﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Turbocharged.Beanstalk
{
    class ListTubesWatchedRequest : Request<List<string>>
    {
        public Task<List<string>> Task { get { return _tcs.Task; } }

        TaskCompletionSource<List<string>> _tcs = new TaskCompletionSource<List<string>>();

        public byte[] ToByteArray()
        {
            return "list-tubes-watched\r\n".ToASCIIByteArray();
        }

        public void Process(string firstLine, NetworkStream stream)
        {
            try
            {
                var parts = firstLine.Split(' ');
                if (parts.Length == 2 && parts[0] == "OK")
                {
                    var bytes = Convert.ToInt32(parts[1]) + 2; // Get the last CRLF
                    var buffer = new byte[bytes];
                    stream.Read(buffer, 0, bytes);
                    var tubes = YamlHelper.ParseList(buffer);
                    _tcs.SetResult(tubes);
                    return;
                }
            }
            catch
            {
            }

            _tcs.SetException(new Exception("Unknown list-tubes-watched response"));
        }

        public void Cancel()
        {
            _tcs.TrySetCanceled();
        }
    }
}
