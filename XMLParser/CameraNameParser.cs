using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using Util;

namespace XMLParser {
    public class CameraNameParser {
        string fileName = null;

        public CameraNameParser(string filename) {
            fileName = filename;
        }

        Encoder coder = new Encoder();

        public void parse(Dictionary<string, string> CameraName2IP, Dictionary<string, string> IP2CameraName, ObservableCollection<CameraNameWrapper> cameraNameList) {

            XmlTextReader reader = new XmlTextReader(fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/CameraNames/Camera");

            foreach (XmlNode node in nodes) {
                string cameraName = node.SelectSingleNode("Name").InnerText;
                string ip = node.SelectSingleNode("IP").InnerText;
                string username = node.SelectSingleNode("Username").InnerText;
                string password = node.SelectSingleNode("Password").InnerText;

                var item = new CameraNameWrapper(cameraName);
                item.AssociatedIP = ip;
                if (ip != "") {
                    CameraName2IP[cameraName] = ip;
                    IP2CameraName[ip] = cameraName;
                    item.NotAssociated = false;
                    string decryptedUsername = coder.Decrypt(username);
                    string decryptedPassword = coder.Decrypt(password);
                    item.username = decryptedUsername;
                    item.password = decryptedPassword;
                } else {
                    item.NotAssociated = true;
                }

                cameraNameList.Add(item);

            }
        }

        public void write(ObservableCollection<CameraNameWrapper> cameraNameList) {

            using (XmlWriter writer = XmlWriter.Create(fileName)) {
                writer.WriteStartDocument();
                writer.WriteStartElement("CameraNames");

                foreach (CameraNameWrapper cam in cameraNameList) {
                    if (cam.CameraName != null || cam.CameraName != null) {
                        writer.WriteStartElement("Camera");
                        writer.WriteElementString("Name", cam.CameraName.ToString());
                        if (cam.NotAssociated == false) {
                            writer.WriteElementString("IP", cam.AssociatedIP.ToString());
                            if (cam.username == null || cam.password == null) {
                                writer.WriteElementString("Username", "");
                                writer.WriteElementString("Password", "");
                            } else {
                                string encryptedUsername = coder.Encrypt(cam.username);
                                string encryptedPassword = coder.Encrypt(cam.password);
                                writer.WriteElementString("Username", encryptedUsername);
                                writer.WriteElementString("Password", encryptedPassword);
                            }

                        } else {
                            writer.WriteElementString("IP", "");
                            writer.WriteElementString("Username", "");
                            writer.WriteElementString("Password", "");
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

        }
    }

}
