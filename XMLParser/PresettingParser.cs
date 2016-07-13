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
        
        public List<PresetParams> parse(List<string> usedCamList) {
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

                    if (!usedCamList.Contains(cam.CameraName)) { usedCamList.Add(cam.CameraName); }
                } catch {
                    Debug.WriteLine("Invalide Format.");
                }
            }

            return cams;
        }
    }
}
