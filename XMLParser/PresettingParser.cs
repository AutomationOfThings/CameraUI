using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Util;
using System.Diagnostics; 

namespace XMLParser {
    public class PresettingParser
    {
        string fileName = null;

        public PresettingParser(string filename) {
            this.fileName = filename;
        }
        
        public List<PresetParams> parse(Dictionary<string, PresetParams> PresetName2Preset) {
            List<PresetParams> cams = new List<PresetParams>();
            XmlTextReader reader = new XmlTextReader(fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Cameras/Preset");

            foreach (XmlNode node in nodes) {
                PresetParams cam = new PresetParams();
                try {
                    cam.presettingId = node.Attributes["Name"].Value.ToString();
                    cam.CameraName = node.SelectSingleNode("CameraID").InnerText;
                    cam.pan = Convert.ToDouble(node.SelectSingleNode("Pan").InnerText);
                    cam.tilt = Convert.ToDouble(node.SelectSingleNode("Tilt").InnerText);
                    cam.zoom = Convert.ToDouble(node.SelectSingleNode("Zoom").InnerText); //node.Attributes["Zoom"].Value;
                    cams.Add(cam);

                    PresetName2Preset[cam.presettingId] = cam;

                } catch {
                    Debug.WriteLine("Invalide Format.");
                }
            }

            return cams;
        }
    }

    public class PresettingWriter {
        string fileName;

        public PresettingWriter(string filename) {
            this.fileName = filename;
        }

        public void write(List<PresetParams> list) {
            using (XmlWriter writer = XmlWriter.Create(fileName)) {
                writer.WriteStartDocument();
                writer.WriteStartElement("Cameras");

                foreach (PresetParams cam in list) {
                    writer.WriteStartElement("Preset");
                    writer.WriteAttributeString("Name", cam.presettingId);
                    writer.WriteElementString("CameraID", cam.CameraName.ToString());
                    writer.WriteElementString("Pan", cam.pan.ToString());
                    writer.WriteElementString("Tilt", cam.tilt.ToString());
                    writer.WriteElementString("Zoom", cam.zoom.ToString());

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
