﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace PubnubApi
{
    public interface IPubnubHttp
    {
        HttpWebRequest SetProxy<T>(HttpWebRequest request);

        HttpWebRequest SetTimeout<T>(RequestState<T> pubnubRequestState, HttpWebRequest request);

        HttpWebRequest SetNoCache<T>(HttpWebRequest request);

        HttpWebRequest SetServicePointSetTcpKeepAlive(HttpWebRequest request);

        Task<string> SendRequestAndGetJsonResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request);

        Task<string> SendRequestAndGetJsonResponseWithPOST<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] postData, string contentType);

        Task<string> SendRequestAndGetJsonResponseWithPATCH<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request, byte[] patchData);

        Task<byte[]> SendRequestAndGetStreamResponse<T>(Uri requestUri, RequestState<T> pubnubRequestState, HttpWebRequest request);
    }

}
