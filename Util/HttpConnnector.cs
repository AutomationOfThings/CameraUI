﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Util {
    class HttpConnnector {
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
                RequestData = new StringBuilder(String.Empty);
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
    }
}
