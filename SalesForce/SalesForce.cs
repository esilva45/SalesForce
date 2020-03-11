using System;
using System.IO;
using System.Xml.Linq;

namespace SalesForce {

    [MyPhonePlugins.CRMPluginLoader]
    public class SalesForce {
        private static SalesForce instance = null;                          // Holds the instance
        private MyPhonePlugins.IMyPhoneCallHandler callHandler = null;      // Holds the handler

        [MyPhonePlugins.CRMPluginInitializer]
        public static void Loader(MyPhonePlugins.IMyPhoneCallHandler callHandler) {
            instance = new SalesForce(callHandler);
        }

        // Constructor for plugin, to add event handler
        private SalesForce(MyPhonePlugins.IMyPhoneCallHandler callHandler) {
            this.callHandler = callHandler;
            
            callHandler.OnCallStatusChanged += new MyPhonePlugins.CallInfoHandler(callHandler_OnCallStatusChanged);
        }

        // Processes the status of the call
        private void callHandler_OnCallStatusChanged(object sender, MyPhonePlugins.CallStatus callInfo) {
            var extensionInfo = sender as MyPhonePlugins.IExtensionInfo;

            switch (callInfo.State) {
                case MyPhonePlugins.CallState.Ended: {
                        XElement configXml = XElement.Load(System.AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
                        string url = configXml.Element("Url").Value.ToString();
                        string path = configXml.Element("PathFile").Value.ToString();
                        string id = searchID(path, callInfo.OtherPartyNumber, extensionInfo.Number);
                        
                        if (!string.IsNullOrEmpty(id)) {
                            url = url + id + "/view/";
                            System.Diagnostics.Process.Start(url);
                        }
                        
                        break;
                };
                case MyPhonePlugins.CallState.Ringing: {
                        break;
                };
                case MyPhonePlugins.CallState.Undefined: {
                        break;
                };
                default: {
                        break;
                };
            }
        }

        private string searchID(string path, string OtherPartyNumber, string number) {
            string id = "";

            using (StreamReader sr = new StreamReader(path)) {
                string searchString = "Chamada de entrada atendida de " + OtherPartyNumber + " para " + number;
                string whoID = "WhoId\":\"";
                int Start, End;

                try {
                    string line;

                    while ((line = sr.ReadLine()) != null) {
                        if (line.Contains(searchString)) {
                            if (line.Contains(whoID)) {
                                Start = line.IndexOf(whoID, 0) + whoID.Length;
                                End = line.IndexOf("\"", Start);
                                id = line.Substring(Start, End - Start);
                            }
                        }
                    }
                }
                catch (Exception) {
                    id = "";
                }
                finally {
                    sr.Close();
                }
            }

            return id;
        }
    }
}
