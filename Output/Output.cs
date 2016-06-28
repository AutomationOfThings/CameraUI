using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using Util;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;

namespace Output {
    public class OutputVM: BindableBase {

        public ModeColors modeColors { get; set; }

        Visibility idle;
        public Visibility Idle {
            get { return idle; }
            set { SetProperty(ref idle, value); }
        }

        Visibility active;
        public Visibility Active {
            get { return active; }
            set { SetProperty(ref active, value); }
        }

        protected readonly IEventAggregator _ea;

        private CameraInfo outputCamera;
        public CameraInfo OutputCamera {
            get { return outputCamera; }
            set { SetProperty(ref outputCamera, value); }
        }

        public OutputVM(IEventAggregator eventAggregator) {
            this.outputCamera = null;
            Idle = Visibility.Visible;
            Active = Visibility.Hidden;
            _ea = eventAggregator;
            _ea.GetEvent<CameraOutPutEvent>()
                .Subscribe(outPutCamera);
        }

        public void outPutCamera(CameraInfo cam) {
            OutputCamera = cam;
            Idle = Visibility.Hidden;
            Active = Visibility.Visible;
        }
    }
}
