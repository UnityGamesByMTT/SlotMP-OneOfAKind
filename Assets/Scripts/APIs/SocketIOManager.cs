using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using UnityEngine.UIElements;

public class SocketIOManager : MonoBehaviour
{
  [Header("Controllers")]
  [SerializeField] private SlotBehaviour slotManager;
  [SerializeField] private UIManager uiManager;
  [Header("Test Token")]
  [SerializeField] private string testToken;
  internal GameData initialData = null;
  internal UIData initUIData = null;
  internal GameData resultData = null;
  internal PlayerData playerdata = null;
  internal bool isResultdone = false;
  [SerializeField] internal JSFunctCalls JSManager;
  protected string nameSpace = "";
  private Socket gameSocket;
  private SocketManager manager;
  protected string SocketURI = null;
  protected string TestSocketURI = "http://localhost:5001/";
  // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
  protected string gameID = "SL-ONE";
  // protected string gameID = "";
  internal bool isLoaded = false;
  internal bool SetInit = false;
  private const int maxReconnectionAttempts = 6;
  private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

  private void Awake()
  {
    //Debug.unityLogger.logEnabled = false;
    isLoaded = false;
    SetInit = false;

  }

  private void Start()
  {
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);

    // Parse the JSON data
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);

    // Proceed with connecting to the server using myAuth and socketURL
    SocketURI = data.socketURL;
    myAuth = data.cookie;
    nameSpace = data.nameSpace;
  }

  string myAuth = null;

  private void OpenSocket()
  {
    // Create and setup SocketOptions
    SocketOptions options = new SocketOptions();
    options.ReconnectionAttempts = maxReconnectionAttempts;
    options.ReconnectionDelay = reconnectionDelay;
    options.Reconnection = true;
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = testToken,
        gameId = gameID
      };
    };
    options.Auth = authFunction;
    // Proceed with connecting to the server
    SetupSocketManager(options);
#endif
  }


  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      Debug.Log("My Socket is null");
      yield return null;
    }
    Debug.Log("My Auth is not null");

    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth,
        gameId = gameID
      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  private void SetupSocketManager(SocketOptions options)
  {
    // Create and setup SocketManager
#if UNITY_EDITOR
    this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif
    if(string.IsNullOrEmpty(nameSpace) | string.IsNullOrWhiteSpace(nameSpace)){
      gameSocket = this.manager.Socket;
    }
    else{
      Debug.Log("Namespace used :"+nameSpace);
      gameSocket = this.manager.GetSocket("/" + nameSpace);
    }
    // Set subscriptions
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    gameSocket.On<string>(SocketIOEventTypes.Disconnect, OnDisconnected);
    gameSocket.On<string>(SocketIOEventTypes.Error, OnError);
    gameSocket.On<string>("message", OnListenEvent);
    gameSocket.On<bool>("socketState", OnSocketState);
    gameSocket.On<string>("internalError", OnSocketError);
    gameSocket.On<string>("alert", OnSocketAlert);
    gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice);
    // Start connecting to the server
  }

  // Connected event handler implementation
  void OnConnected(ConnectResponse resp)
  {
    Debug.Log("Connected!");
    SendPing();
  }

  private void OnDisconnected(string response)
  {
    Debug.Log("Disconnected from the server");
    StopAllCoroutines();
    uiManager.DisconnectionPopup(false);
  }

  private void OnError(string response)
  {
    Debug.LogError("Error: " + response);
  }

  private void OnListenEvent(string data)
  {
    ParseResponse(data);
  }

  private void OnSocketState(bool state)
  {
    Debug.Log("my state is " + state);
  }

  private void OnSocketError(string data)
  {
    Debug.Log("Received error with data: " + data);
  }

  private void OnSocketAlert(string data)
  {
    Debug.Log("Received alert with data: " + data);
  }

  private void OnSocketOtherDevice(string data)
  {
    Debug.Log("Received Device Error with data: " + data);
    uiManager.ADfunction();
  }

  private void SendPing()
  {
    InvokeRepeating("AliveRequest", 0f, 3f);
  }

  private void AliveRequest()
  {
    SendDataWithNamespace("YES I AM ALIVE");
  }

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (gameSocket != null && gameSocket.IsOpen)
    {
      if (json != null)
      {
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        gameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }

  internal void CloseSocket()
  {
    SendDataWithNamespace("EXIT");
  }
  internal void closeSocketReactnativeCall()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
  JSManager.SendCustomMessage("onExit");
#endif
  }
  private void ParseResponse(string jsonObject)
  {
    Debug.Log(jsonObject);
    Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

    string id = myData.id;

    switch (id)
    {
      case "InitData":
        {
          initialData = myData.message.GameData;
          initUIData = myData.message.UIData;
          playerdata = myData.message.PlayerData;
          // bonusdata = myData.message.BonusData;
          // LineData = myData.message.GameData.Lines;
          if (!SetInit)
          {
            //Debug.Log(jsonObject);
            PopulateSlotSocket();
            SetInit = true;
          }
          else
          {
            RefreshUI();
          }
          break;
        }
      case "ResultData":
        {
          resultData = myData.message.GameData;
          playerdata = myData.message.PlayerData;
          isResultdone = true;
          break;
        }
      case "ExitUser":
        {
          gameSocket.Disconnect();
          if (this.manager != null)
          {
              Debug.Log("Dispose my Socket");
              this.manager.Close();
          }
#if UNITY_WEBGL && !UNITY_EDITOR
          JSManager.SendCustomMessage("onExit");
#endif
          break;
        }
    }
  }

  private void RefreshUI()
  {
    uiManager.InitialiseUIData(initUIData.AbtLogo.link, initUIData.AbtLogo.logoSprite, initUIData.ToULink, initUIData.PopLink, initUIData.paylines);
  }

  private void PopulateSlotSocket()
  {
    slotManager.ShuffleSlot();

    slotManager.SetInitialUI();

    isLoaded = true;
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnEnter");
#endif

  }

  internal void AccumulateResult(double currBet)
  {
    isResultdone = false;
    MessageData message = new MessageData();
    message.data = new BetData();
    message.data.currentBet = currBet;
    message.data.spins = 1;
    message.data.currentLines = 1;
    message.id = "SPIN";

    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("message", json);
  }

  private List<string> RemoveQuotes(List<string> stringList)
  {
    for (int i = 0; i < stringList.Count; i++)
    {
      stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
    }
    return stringList;
  }

  private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
  {
    List<string> resultList = new List<string>();

    foreach (List<int> innerList in listOfLists)
    {
      // Convert each integer in the inner list to string
      List<string> stringList = new List<string>();
      foreach (int number in innerList)
      {
        stringList.Add(number.ToString());
      }

      // Join the string representation of integers with ","
      string joinedString = string.Join(",", stringList.ToArray()).Trim();
      resultList.Add(joinedString);
    }

    return resultList;
  }

  private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
  {
    List<string> outputList = new List<string>();

    foreach (List<string> row in inputList)
    {
      string concatenatedString = string.Join(",", row);
      outputList.Add(concatenatedString);
    }

    return outputList;
  }

  private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
  {
    // Flattened list
    List<string> flattenedList = new List<string>();
    foreach (List<string> sublist in originalList)
    {
      flattenedList.AddRange(sublist);
    }

    // Remove recurring elements
    HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

    // Transformed list
    List<string> transformedList = new List<string>();
    foreach (string element in uniqueElements)
    {
      transformedList.Add(element.Replace(",", ""));
    }

    return transformedList;
  }
}

[Serializable]
public class BetData
{
  public double currentBet;
  public double currentLines;
  public double spins;
}

[Serializable]
public class AuthData
{
  public string GameID;
}

[Serializable]
public class MessageData
{
  public BetData data;
  public string id;
}

[Serializable]
public class ExitData
{
  public string id;
}

[Serializable]
public class InitData
{
  public AuthData Data;
  public string id;
}

[Serializable]
public class AbtLogo
{
  public string logoSprite { get; set; }
  public string link { get; set; }
}

[Serializable]
public class GameData
{
  public List<double> Bets { get; set; }
  public List<int> LevelUp { get; set; }
  public List<int> Booster { get; set; }
  public List<double> Joker { get; set; }
  public int resultSymbols { get; set; }
  public JokerResponse jokerResponse { get; set; }
  public Levelup levelup { get; set; }
  public Booster booster { get; set; }
  public string freespinType { get; set; }
  public FreeSpinResponse freeSpinResponse { get; set; }
}

[Serializable]
public class JokerResponse
{
  public bool isTriggered { get; set; }
  public List<double> payout { get; set; }
  public int blueRound { get; set; }
  public int greenRound { get; set; }
  public int redRound { get; set; }
}

[Serializable]
public class Levelup
{
  public int level { get; set; }
  public bool isLevelUp { get; set; }
}

[Serializable]
public class Booster
{
  public string type { get; set; }
  public List<int> multipliers { get; set; }
}

[Serializable]
public class FreeSpinResponse
{
  public bool isTriggered { get; set; }
  public List<List<int>> topSymbols { get; set; }
  public List<int> symbols { get; set; }
  public double payout { get; set; }
  public List<Levelup> levelUp { get; set; }
  public List<Booster> booster { get; set; }
  public List<int> reTriggered { get; set; }
  public List<int> count { get; set; }
}

[Serializable]
public class Message
{
  public GameData GameData { get; set; }
  public UIData UIData { get; set; }
  public PlayerData PlayerData { get; set; }
}

[Serializable]
public class Root
{
  public string id { get; set; }
  public Message message { get; set; }
  public string username { get; set; }
}

[Serializable]
public class UIData
{
  public Paylines paylines { get; set; }
  public List<string> spclSymbolTxt { get; set; }
  public AbtLogo AbtLogo { get; set; }
  public string ToULink { get; set; }
  public string PopLink { get; set; }
}

[Serializable]
public class Paylines
{
  public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Symbol
{
  public int ID { get; set; }
  public string Name { get; set; }
  [JsonProperty("multiplier")]
  public object MultiplierObject { get; set; }

  // This property will hold the properly deserialized list of lists of integers
  [JsonIgnore]
  public List<List<int>> Multiplier { get; private set; }

  // Custom deserialization method to handle the conversion
  [OnDeserialized]
  internal void OnDeserializedMethod(StreamingContext context)
  {
    // Handle the case where multiplier is an object (empty in JSON)
    if (MultiplierObject is JObject)
    {
      Multiplier = new List<List<int>>();
    }
    else
    {
      // Deserialize normally assuming it's an array of arrays
      Multiplier = JsonConvert.DeserializeObject<List<List<int>>>(MultiplierObject.ToString());
    }
  }
  public object defaultAmount { get; set; }
  public object symbolsCount { get; set; }
  public object increaseValue { get; set; }
  public object description { get; set; }
  public double payout { get; set; }
}

[Serializable]
public class PlayerData
{
  public double Balance { get; set; }
  public double currentWining { get; set; }
  public double totalbet { get; set; }
  public double haveWon { get; set; }
}

[Serializable]
public class AuthTokenData
{
  public string cookie;
  public string socketURL;
  public string nameSpace = "";
}


