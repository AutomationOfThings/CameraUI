using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism = Microsoft.Practices.Prism.PubSubEvents;

namespace NotificationCenter {

    public sealed class Notification {
        private Notification() { }

        private static readonly Prism.EventAggregator _instance = new Prism.EventAggregator();

        public static Prism.EventAggregator Instance { get { return _instance; } }

        private Prism.IEventAggregator _eventAggregator;
        internal Prism.IEventAggregator EventAggregator {
            get {
                if (_eventAggregator == null)
                    _eventAggregator = new Prism.EventAggregator();
                return _eventAggregator;
            }
        }
    }
}
