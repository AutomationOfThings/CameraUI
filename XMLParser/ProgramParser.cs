using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using Util;

namespace XMLParser {
    public class ProgramParser {
        string fileName = null;

        public ProgramParser(string filename) {
            this.fileName = filename;
        }

        public List<List<CameraCommand>> parse() {
            List<List<CameraCommand>> programs = new List<List<CameraCommand>>();
            XmlTextReader reader = new XmlTextReader(fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Programs/Program");

            foreach (XmlNode node in nodes) {
                List<CameraCommand> program = new List<CameraCommand>();
                foreach (XmlNode camera in node.SelectNodes("Step")) {
                    CameraCommand cam = new CameraCommand();
                    cam.Command = fromString(camera.SelectSingleNode("Command").InnerText);
                    if (cam.Command == Cmd.OUTPUT) {
                        cam.SelectedOutputCamera = camera.SelectSingleNode("Parameter").InnerText;
                    } else if (cam.Command == Cmd.PRESET) {
                        cam.SelectedPreset = camera.SelectSingleNode("Parameter").InnerText;
                    } else {
                        cam.WaitTime = Convert.ToDouble(camera.SelectSingleNode("Parameter").InnerText);
                    }
                    program.Add(cam);
                }
                programs.Add(program);
            }

            return programs;
        }

        public Cmd fromString(string cmd) {
            if (cmd == "OUTPUT")
                return Cmd.OUTPUT;
            else if (cmd == "PRESET")
                return Cmd.PRESET;
            else
                return Cmd.WAIT;               
        }

    }
}
