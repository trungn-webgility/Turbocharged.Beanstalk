﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Turbocharged.Beanstalk
{
    class TouchRequest : Request<bool>
    {
        public Task<bool> Task { get { return _tcs.Task; } }

        TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        int _id;

        public TouchRequest(int id)
        {
            _id = id;
        }

        public byte[] ToByteArray()
        {
            return "touch {0}\r\n".FormatWith(_id).ToASCIIByteArray();
        }

        public void Process(string firstLine, NetworkStream stream, ILogger logger)
        {
            switch (firstLine)
            {
                case "TOUCHED":
                    _tcs.SetResult(true);
                    return;

                case "NOT_FOUND":
                    _tcs.SetResult(false);
                    return;

                default:
                    Reply.SetGeneralException(_tcs, firstLine, "touch", logger);
                    return;
            }
        }

        public void Cancel()
        {
            _tcs.TrySetCanceled();
        }
    }
}
