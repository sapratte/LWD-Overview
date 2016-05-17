using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using SOD_CS_Library;

namespace ClientApp{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window {
        private string address = "192.168.20.96";
        //private string address = "10.12.35.161";
        private int port = 3000;
        string base64buffer = "";
        int flag = 0;
        public SOD_CS_Library.SOD SOD;
        Image1 w;
        Point original;
        bool firstTime = true;
        System.Timers.Timer aTimer = new System.Timers.Timer(35);
        public MainWindow() {
            w = new Image1();
            InitializeComponent();
            try {
                this.SOD = new SOD_CS_Library.SOD(address,port);
                SOD.LoadExistingConfig();
                Execute();

                //SOD.SocketConnect();



                /*TcpClient tc = new TcpClient();
                Console.WriteLine("connecting...");
                tc.Connect("192.168.0.192", 8001);
                Console.WriteLine("Connected");
                Console.Write("Enter the string to be transmitted : ");

                //String str = Console.ReadLine();
                String str = "32.1 + 45";
                Stream stm = tc.GetStream();
                ASCIIEncoding asc = new ASCIIEncoding();
                byte[] bt = asc.GetBytes(str);
                Console.WriteLine("transmitting...");
                stm.Write(bt, 0, bt.Length);

                byte[] bt2 = new byte[100];
                int k = stm.Read(bt2, 0, 100);
                for (int i = 0; i < k; i++) {
                    Console.Write(Convert.ToChar(bt2[i]));
                }
                tc.Close();*/
            } catch (Exception e) {
                Console.WriteLine("Error... {0}", e.StackTrace);
            }
        }
        public void Execute() {
            SOD.On("connect", (msgReceived) => {
                Console.WriteLine("\r\nConnected...\r\n");
                Console.WriteLine("\r\nRegistering with server...\r\n");
                updateStatus("\r\nConnected...\r\n");
                updateStatus("\r\nRegistering with server...\r\n");
                Console.WriteLine(msgReceived.ToString() + "!!");

                //this.SOD.ownDevice.observeRange = 1; Deprecated
                this.SOD.ownDevice.locationX = 1;
                this.SOD.ownDevice.locationY = 0;
                this.SOD.ownDevice.locationZ = 1;
                //this.SOD.ownDevice.orientation = 45;

                this.SOD.ownDevice.width = 2;
                this.SOD.ownDevice.height = 1;
                this.SOD.ownDevice.depth = 0.5;


                // rectangular observer: constructor(width,height,distance) in meters

                this.SOD.ownDevice.observer = new observer(2, 1, 2);
                // radial observer : constructor(radius) in meters
                this.SOD.ownDevice.observer = new observer(1);
                this.SOD.ownDevice.FOV = 70;



                this.SOD.RegisterDevice();  //register the device with server everytime it connects or re-connects
                                            //this.SOD.StartSendingOrientation(200);

                //any other code/methods which should be executed when socket is connected to server//


            });
            SOD.On("check", (msgReceived) => {

                Dispatcher.BeginInvoke(new ThreadStart(() => {
                    if (txtTestData.Text != "" && flag == 0) {
                        Console.WriteLine("checkstring");
                        SOD.SendToDevices.All("string", base64buffer);
                        flag = 1;
                    }
                }));



                //send string off to all devices

                //SOD.SendStringToDevices(stringToSend, "all");
                //SOD.SendToDevices.All("string", stringToSend);



            });


            // SOD default events handlers. addin before connect to server.
            SOD.OnConnect += socketConnected;
            SOD.OnRegister += clientRegistered;
            SOD.OnString += stringReceived;

            SOD.OnDictionaryReceived += dictionaryReceived;
            SOD.OnSomeDeviceConnected += showSomeDeviceConnected;
            SOD.OnSomeDeviceDisconnected += showSomeDeviceDisconnected;

            SOD.OnEnterObserverRange += enterObserverRangeOccured;
            SOD.OnLeaveObserverRange += leaveObserverRangeOccured;
            SOD.OnEnterView += enterViewOccured;
            SOD.OnLeaveView += leaveViewOccured;

            SOD.On("string", (msgReceived) => {
                Console.WriteLine("0000000000000000000000000000000000000");
                

                updateStatus("You were sent a string: " + msgReceived.data);
                Console.WriteLine("11111111111111111111111111111111111111");
                if (msgReceived.data.Length > 100) {
                    byte[] bytes = Convert.FromBase64String(msgReceived.data);
                    Console.WriteLine("22222222222222222222222222222222222222");
                    System.IO.File.WriteAllBytes(@"C:\Users\Matheus Fernandes\Documents\Visual Studio 2015\Projects\SoD_CS_Sample_Client-master\SampleSODClient\new.png", bytes);
                    Console.WriteLine("33333333333333333333333333333333333333");
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        image1.Source = new BitmapImage(new Uri(@"C:\Users\Matheus Fernandes\Documents\Visual Studio 2015\Projects\SoD_CS_Sample_Client-master\SampleSODClient\new.png"));
                        Console.WriteLine("44444444444444444444444444444444444444");

                    }));
                }
                
            });

            SOD.On("SuperSecretData", (msgReceived) => {
                updateStatus("You were sent the data you requested: " + msgReceived.data);
            });

            SOD.On("request", (msgReceived) => {
                //OPTIONAL, just shows how to access the fields from a parsed message
                object requestedDataName = msgReceived.data;
                int PID = msgReceived.PID;

                updateStatus("You were asked for this data: " + requestedDataName);

                //sample dictionary to send back as a reply
                Dictionary<string, string> dataToSendBack = new Dictionary<string, string>(){
                    { "SecretDataA", "Shhhhh" },
                    { "SecretDataB", "Here is the secret data you requested from the C# client" }
                };
                Console.WriteLine("PID: " + PID);
                //if PID was parsed successfully from the received message, send back a reply/acknowledgement with the same PID
                if (PID != -1) {
                    SOD.SendAcknowledgementWithPID(PID, dataToSendBack);
                }
            });

           
            aTimer.Elapsed += OnTimedEvent;
            aTimer.Enabled = true;
            aTimer.AutoReset = true;


            SOD.ownDevice.SetDeviceInformation(0.5, 0.5, 1, 2, 3, "walldisplay", true);
            SOD.ownDevice.stationary = false;
            SOD.ownDevice.FOV = 30;
            SOD.SocketConnect();
            w.SOD = this.SOD;
            w.Show();
        }

        private void leaveViewOccured(object sender, SOD.ObserverVistiorEventArgs e) {
            updateStatus(" " + e.visitor.type + " ID: " + e.visitor.ID + " leaves view of " + e.observer.type + ": " + e.observer.ID);
        }

        private void enterViewOccured(object sender, SOD.ObserverVistiorEventArgs e) {
            SOD.GetAllTrackedPeople((people) => {
                if (people.Count != 0) {
                    original = new Point(people[0].location.X, people[0].location.Z);
                }
            });
            updateStatus(" " + e.visitor.type + " ID: " + e.visitor.ID + " enters View of " + e.observer.type + ": " + e.observer.ID);
        }

        private void leaveObserverRangeOccured(object sender, SOD.ObserverVistiorEventArgs e) {
            updateStatus(" " + e.visitor.type + " ID: " + e.visitor.ID +" leaves " + e.observer.type + ": " + e.observer.ID);
        }

        private void enterObserverRangeOccured(object sender, SOD.ObserverVistiorEventArgs e) {
            SOD.GetAllTrackedPeople((people) => {
                if (people.Count != 0) {
                    original = new Point(people[0].location.X, people[0].location.Z);
                }
            });
            updateStatus(" " + e.visitor.type + " ID: " + e.visitor.ID +" enter " + e.observer.type + ": " + e.observer.ID);
        }

        public void socketConnected(object sender, SOD.CallbackEventArgs e) {
            updateStatus("Socket Connected");
        }

        public void clientRegistered(object sender, SOD.OnRegisterEventArgs e) {
            Console.WriteLine("Client Registered with status code: " + e.jsonData.ToString());
            updateStatus(" Register Status: " + e.status);

            Console.WriteLine("HERE ________________ SEND");
            Console.WriteLine("WINDOW WIDTH " + w.ActualWidth + " HEIGHT " + w.ActualHeight);

            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                Dictionary<string, string> dataToSendBack = new Dictionary<string, string>(){
                    { "width", w.image.ActualWidth.ToString() },
                    { "height", w.image.ActualHeight.ToString() },
                    {"round", w.round.ToString()}
                };
                this.SOD.SendToDevices.All("setImgSize", dataToSendBack);
            }));
        }

        public void stringReceived(object sender, SOD.onStringEventArgs e) {
            String s = e.stringData;
            if(!s.Contains("pointtag;"))
                updateStatus("String received: " + " yess??:: " + e.stringData);
            else {//it's the new point
                Dispatcher.Invoke((Action)(() => {
                    String[] s1 = s.Split(';');
                    Point p = new Point();
                    p.X = Double.Parse(s1[1]);
                    p.Y = Double.Parse(s1[2]);
                    w.setNewPoint(p);
                }));
               
            }

        }
        public void dictionaryReceived(object sender, SOD.onDictionaryReceivedEventArgs e) {
            updateStatus("You were sent a dictionary: " + e.dictionaryData);
        }

        public void showSomeDeviceConnected(object sender, SOD.DeviceInfoEventArgs e) {
            updateStatus("" + e.deviceType + " " + e.ID + " with name: " + e.name + " connected to server");
        }

        public void showSomeDeviceDisconnected(object sender, SOD.DeviceInfoEventArgs e) {
            updateStatus("" + e.deviceType + " " + e.ID + " with name: " + e.name + " disconnected from server");
        }
        private void updateStatus(string textToAppend) {
            Dispatcher.Invoke((Action)(() => {
                txtStatus.Text = txtStatus.Text + "\n" + textToAppend;
                scvStatus.ScrollToBottom();
            }));
        }

        private void btnSendTestData_Click(object sender, RoutedEventArgs e) {
            Dictionary<string, string> deviceinfo = new Dictionary<string, string>(){
                    { "name", txtNameOfDevice.Text }
                };
            SOD.updateDeviceInfo(deviceinfo);

            string stringToSend = txtTestData.Text;

            //send string off to all devices

            //SOD.SendStringToDevices(stringToSend, "all");
            //SOD.SendToDevices.All("string", stringToSend);

            SOD.SendToDevices.All("string", stringToSend);


            //SOD.SendStringToDevices(stringToSend, new string[3]{"all", "all", "all"});
            filterDefinition filterList = new filterDefinition();
            filterList.AddFilter.All();
            filterList.AddFilter.All();
            filterList.AddFilter.All();
            //SOD.SendToDevices.CustomFilter(filterList, "string", stringToSend);

        }

        private void btnSendRequest_Click(object sender, RoutedEventArgs e) {
            //set the requestName (ie. what data is being requested)
            string requestName = "SuperSecretData";

            //send the request to all devices

            //SOD.RequestDataFromSelection(requestName, "all");
            SOD.RequestFromDevices.All(requestName);
        }

        private void btnReconnect_Click(object sender, RoutedEventArgs e) {
            SOD.ReconnectToServer();
        }

        private void btnGetPeople_Click(object sender, RoutedEventArgs e) {
            SOD.GetAllTrackedPeople(UpdateStatusWithPeople);
        }

        private void btnGetDevicesInView_Click(object sender, RoutedEventArgs e) {
            SOD.GetDevicesWithSelection("inView", UpdateStatusWithDevices);
            SOD.GetDevicesInViewWithDistance((callback) => {
                foreach (SOD.deviceWithDistanceEncapsulate encap in callback)
                    Console.WriteLine(encap.device.ID + " is inView with distance: " + encap.distance);
            });
        }

        private void btnGetDevicesByType_Click(object sender, RoutedEventArgs e) {
            SOD.GetDevicesWithSelection("all", FilterByType);
        }

        public void FilterByType(List<Device> devices) {
            Console.WriteLine("\n*** Devices Returned ***");
            Console.WriteLine("There are " + devices.Count() + " recieved.");
            updateStatus("\n*** Devices Returned ***");
            foreach (Device device in devices) {
                Console.WriteLine(device.deviceType);
                this.Dispatcher.Invoke((Action)(() => // check whether current thread owns this code
                {
                    if (device.deviceType.Equals(txtDeviceType.Text)) {
                        Console.WriteLine("->Device ID: " + device.ID + "\t*type: " + device.deviceType + " *IP: " + device.deviceIP);
                        updateStatus("->Device ID: " + device.ID + "\t*type: " + device.deviceType + " *IP: " + device.deviceIP);
                    }
                }));
            }
        }

        private void btnGetDeviceByID_Click(object sender, RoutedEventArgs e) {
            int ID;
            //make sure specified ID in textbox is a valid int
            if (Int32.TryParse(txtTestID.Text, out ID)) {
                Console.WriteLine("ID: " + ID);
                SOD.GetDeviceByID(ID, UpdateStatusWithDevice);
            } else {
                Console.WriteLine("Specified ID is not an integer.");
                updateStatus("Specified ID is not an integer.");
            }
        }

        public void UpdateStatusWithDevice(Device device) {
            if (device != null) {
                Console.WriteLine("\n*** Device Returned ***");
                Console.WriteLine("->Device ID: " + device.ID + "\t*type: " + device.deviceType + " *IP: " + device.deviceIP);
                updateStatus("\n*** Device Returned ***");
                updateStatus("->Device ID: " + device.ID + "\t*type: " + device.deviceType + " *IP: " + device.deviceIP);
            } else {
                Console.WriteLine("\n*** No device found with ID specified! ***");
                updateStatus("\n*** No device found with ID specified! ***");
            }
        }

        public void UpdateStatusWithDevices(List<Device> devices) {
            Console.WriteLine("\n*** Devices Returned ***");
            Console.WriteLine("There are " + devices.Count() + " received.");
            updateStatus("\n*** Devices Returned ***");
            updateStatus("There are " + devices.Count() + " received.");
            foreach (Device device in devices) {
                Console.WriteLine("->Device ID: " + device.ID + "\t*type: " + device.deviceType + " *IP: " + device.deviceIP);
                updateStatus("->Device ID: " + device.ID + "\t*type: " + device.deviceType + " *IP: " + device.deviceIP);
            }
        }

        public void UpdateStatusWithPeople(List<Person> people) {
            //Console.WriteLine(people);

            //interate through people list
            foreach (Person aPerson in people) {
                Console.WriteLine(aPerson.ToString());
                updateStatus(aPerson.ToString());
            }
        }


        private void btnSendTestDictionarySingle_Click(object sender, RoutedEventArgs e) {
            int ID;
            //grab a string from app's textbox
            string stringToSend = txtTestData.Text;

            //package string into a sample dictionary
            Dictionary<string, string> dictionaryToSend = new Dictionary<string, string>(){
                { "Chapter 1", "The Chicken and the Egg" },
                { "Appendix A", stringToSend }
            };

            if (Int32.TryParse(txtTestID.Text, out ID)) {
                //SOD.SendDictionaryToDevices(dictionaryToSend, "single" + ID);
                SOD.SendToDevices.WithID(ID, "dictionary", dictionaryToSend);
            } else {
                Console.WriteLine("Specified ID is not an integer.");
                updateStatus("Specified ID is not an integer.");
            }


            //send dictionary off to all devices
        }


        private void Button_Click(object sender, RoutedEventArgs e) {
            byte[] bytes = System.IO.File.ReadAllBytes(@"C:\Users\Matheus Fernandes\Documents\Visual Studio 2015\Projects\SoD_CS_Sample_Client-master\SampleSODClient\hehe.jpg");
            base64buffer = Convert.ToBase64String(bytes);
            image1.Source = new BitmapImage(new Uri(@"C:\Users\Matheus Fernandes\Documents\Visual Studio 2015\Projects\SoD_CS_Sample_Client-master\SampleSODClient\hehe.jpg"));
            System.IO.File.WriteAllText(@"C:\Users\aselab\Desktop\c# client\SoD_CS_Sample_Client\SampleSODClient\face.txt", base64buffer);
            Console.WriteLine("nihaoya" + base64buffer);


        }


        private void btnSendTestDictionary_Click(object sender, RoutedEventArgs e) {
            //grab a string from app's textbox
            string stringToSend = txtTestData.Text;

            //package string into a sample dictionary
            Dictionary<string, string> dictionaryToSend = new Dictionary<string, string>(){
                { "name", "dictionary" },
                { "data", stringToSend }
            };

            //send dictionary off to all devices

            //SOD.SendDictionaryToDevices(dictionaryToSend, "all");
            SOD.SendToDevices.All("dictionary", dictionaryToSend);

        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e) {
            SOD.GetAllTrackedPeople((people) => {
                if (people.Count != 0) {
                    
                    if (firstTime) {
                        original = new Point(0,0);
                        firstTime = false;
                    } else if(original!=null) {

                        Person p = people[0];
                        double px = 440 * p.location.X / 0.82;
                        double py = 25 * 8 * (p.location.Z + 0.45);
                        py -= 400;

                        //lateral distance to which the movement should be proportional
                        double distX = Math.Sqrt(2 * Math.Pow(p.location.Z, 2)*(1 - Math.Cos(57*Math.PI/180)));

                        //euclidean distance sqrt((x1-x2)^2+(y1-y2)^2+(z1-z2)^2) between (x1,y1,z1) and (x2,y2,z2)
                        if (Math.Sqrt(Math.Pow(px - original.X,2))/distX>Math.Sqrt(Math.Pow(py - original.Y,2))/3.2) {//moved horizontally
                            Dispatcher.Invoke((Action)(() => {
                                original.X = px;
                                w.setNewPoint(original);
                                Console.WriteLine("CHANGE IN X");
                            }));
                       // } else if (p.location.Z != original.Y) {//moved backwards or forwards
                           
                        } else {
                            Dispatcher.Invoke((Action)(() => {
                                Console.WriteLine("CHANGE IN Z");
                                original.Y = -py;
                                w.setNewPoint(original);
                            }));
                            //Console.WriteLine("NAO MUDOU? NAO MUDOU?");
                        }//didn't move otherwise
                    }else {
                        firstTime = true;
                    }
                }

            });
        }

        private void btnSendTestDictionaryIncludingSelf_Click(object sender, RoutedEventArgs e) {
            //grab a string from app's textbox
            string stringToSend = txtTestData.Text;

            //package string into a sample dictionary
            Dictionary<string, string> dictionaryToSend = new Dictionary<string, string>(){
                { "name", "dictionary" },
                { "data", stringToSend }
            };

            //send dictionary off to all devices

            //SOD.SendDictionaryToDevicesIncludingSelf(dictionaryToSend, "all");
            SOD.SendToDevices.AllIncludingSelf("dictionary", dictionaryToSend);
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e) {
            SOD.Close();
        }

        private void btnGetAllDevices_Click(object sender, RoutedEventArgs e) {
            SOD.GetDevicesWithSelection("all", UpdateStatusWithDevices);
            //SOD.GetDevicesWithSelection(new string[3]{"all", "all", "all"}, UpdateStatusWithDevices);
        }

    }
}
