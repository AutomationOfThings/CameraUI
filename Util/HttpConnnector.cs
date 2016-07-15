
using LCM.LCM;
using Microsoft.Practices.Prism.PubSubEvents;
using NotificationCenter;
using ptz_camera;

namespace Util {
    public class CameraConnnector {

        private static readonly LCM.LCM.LCM _lcm = LCM.LCM.LCM.Singleton;
        private EventAggregator _ea;

        public CameraConnnector() {
            _ea = Notification.Instance;
        }

        public static void initializeSession(string ip, string un, string pwd) {
            var initSessionRequest = new init_session_request_t() {
                ip_address = ip,
                username = un,
                password = pwd
            };
            _lcm.Publish(Channels.init_session_req_channel, initSessionRequest);
        }

        public static void endSession(string ip) {
            var endSessionRequest = new end_session_request_t() {
                ip_address = ip
            };
            _lcm.Publish(Channels.end_session_req_channel, endSessionRequest);
        }

        public static void getStreamUri(string ip) {
            var streamUriRequest = new stream_uri_request_t() {
                ip_address = ip,
                profile = "1",
                codec_type = "",
                resolution = "",
                frame_rate = "",
                compression_level = "",
                channel = ""

            };
            _lcm.Publish(Channels.stream_req_channel, streamUriRequest);
        }

        public static void requestCameraPosition(string ip) {
            var positionRequest = new position_request_t() {
                ip_address = ip
            };
            _lcm.Publish(Channels.position_req_channel, positionRequest);
        }

        public static void requestPtzControl(string ip, PTZ_MODE mode, int pan, int tilt, int zoom) {

            var ptzRequest = new ptz_control_request_t() {
                ip_address = ip,
                pan_value = pan.ToString(),
                tilt_value = tilt.ToString(),
                zoom_value = zoom.ToString()
            };

            if (mode == PTZ_MODE.Absolute) {
                ptzRequest.mode = 1;
            } else if (mode == PTZ_MODE.Relative) {
                ptzRequest.mode = 2;
            }
            _lcm.Publish(Channels.ptz_control_req_channel, ptzRequest);
        }

    }


    public class StreamUriResponseHandler: LCMSubscriber {
        public void MessageReceived(LCM.LCM.LCM lcm, string channel, LCMDataInputStream data_stream) {
            if (channel == Channels.stream_res_channel) {
                stream_uri_response_t response = new stream_uri_response_t(data_stream);
                var _ea = Notification.Instance;
                _ea.GetEvent<StreamUriResponseReceivedEvent>().Publish(response);
            }
        }
    }

    public class PositionResponseHandler : LCMSubscriber {
        public void MessageReceived(LCM.LCM.LCM lcm, string channel, LCMDataInputStream data_stream) {
            if (channel == Channels.position_res_channel) {
                position_response_t response = new position_response_t(data_stream);
                var _ea = Notification.Instance;
                _ea.GetEvent<PositionResponseReceivedEvent>().Publish(response);
            }
        }
    }

    public class InitSessionResponseHandler : LCMSubscriber {
        public void MessageReceived(LCM.LCM.LCM lcm, string channel, LCMDataInputStream data_stream) {
            if (channel == Channels.init_session_res_channel) {
                init_session_response_t response = new init_session_response_t(data_stream);
                var _ea = Notification.Instance;
                _ea.GetEvent<InitSessionResponseReceivedEvent>().Publish(response);
            }
        }
    }

    public class EndSessionResponseHandler : LCMSubscriber {
        public void MessageReceived(LCM.LCM.LCM lcm, string channel, LCMDataInputStream data_stream) {
            if (channel == Channels.end_session_res_channel) {
                end_session_response_t response = new end_session_response_t(data_stream);
                var _ea = Notification.Instance;
                _ea.GetEvent<EndSessionResponseReceivedEvent>().Publish(response);
            }
        }
    }

    /*
    public class HttpRequestState {
        const int BufferSize = 1024;
        public StringBuilder RequestData;
        public byte[] BufferRead;
        public WebRequest Request;
        public Stream ResponseStream;
        // Create Decoder for appropriate enconding type.
        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();
        public Action<string> callBack;
        public HttpRequestState(Action<string> cb) {
            BufferRead = new byte[BufferSize];
            RequestData = new StringBuilder(string.Empty);
            Request = null;
            ResponseStream = null;
            callBack = cb;
        }
    }

    public static class HttpConnector {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        const int BUFFER_SIZE = 1024;

        public static string username;
        public static string password;

        public static void requestUseHTTP(string completeURL, Action<string> callback) {
            try {
                HttpWebRequest requester = WebRequest.CreateHttp(completeURL);
                requester.Credentials = new NetworkCredential(username, password);
                HttpRequestState rs = new HttpRequestState(callback);
                rs.Request = requester;
                IAsyncResult r = requester.BeginGetResponse(new AsyncCallback(GetRequestStreamCallback), rs);
            } catch(Exception e) {
                Debug.WriteLine(e.Message);
            }

        }

        public static void GetRequestStreamCallback(IAsyncResult ar) {
            try {
                HttpRequestState rs = (HttpRequestState)ar.AsyncState;

                // Get the WebRequest from RequestState.
                WebRequest req = rs.Request;

                // Call EndGetResponse, which produces the WebResponse object that came from the request issued above.
                WebResponse resp = req.EndGetResponse(ar);

                //  Start reading data from the response stream.
                Stream ResponseStream = resp.GetResponseStream();

                // Store the response stream in RequestState to read the stream asynchronously.
                rs.ResponseStream = ResponseStream;

                //Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
                IAsyncResult iarRead = ResponseStream.BeginRead(rs.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), rs);
            } catch(Exception e) {
                Debug.WriteLine(e.Message);
            }

        }

        private static void ReadCallBack(IAsyncResult asyncResult) {
            try {
                // Get the RequestState object from AsyncResult.
                HttpRequestState rs = (HttpRequestState)asyncResult.AsyncState;

                // Retrieve the ResponseStream that was set in RespCallback. 
                Stream responseStream = rs.ResponseStream;

                // Read rs.BufferRead to verify that it contains data. 
                int read = responseStream.EndRead(asyncResult);
                if (read > 0) {
                    // Prepare a Char array buffer for converting to Unicode.
                    char[] charBuffer = new char[BUFFER_SIZE];

                    // Convert byte stream to Char array and then to String.
                    // len contains the number of characters converted to Unicode.
                    int len = rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);

                    string str = new string(charBuffer, 0, len);

                    // Append the recently read data to the RequestData stringbuilder object contained in RequestState.
                    rs.RequestData.Append(Encoding.ASCII.GetString(rs.BufferRead, 0, read));

                    // Continue reading data until responseStream.EndRead returns –1.
                    IAsyncResult ar = responseStream.BeginRead(
                       rs.BufferRead, 0, BUFFER_SIZE,
                       new AsyncCallback(ReadCallBack), rs);
                } else {
                    if (rs.RequestData.Length > 0) {
                        // get the response data content
                        string strContent = rs.RequestData.ToString();
                        rs.callBack(strContent);
                    }
                    // Close down the response stream.
                    responseStream.Close();
                    // Set the ManualResetEvent so the main thread can exit.
                    allDone.Set();
                }
                return;
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
            }
        }

    }
    */
}
