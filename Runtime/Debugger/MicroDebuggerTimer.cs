#if MICRO_GRAPH_DEBUG
using System;
using System.Diagnostics;
using UnityEngine;

namespace MicroGraph.Runtime
{
    public class MicroDebuggerTimer : MonoBehaviour
    {
        private static MicroDebuggerTimer _instance;
        public static MicroDebuggerTimer Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject(nameof(MicroDebuggerTimer));
                    GameObject.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<MicroDebuggerTimer>();
                }
                return _instance;
            }
        }
    }

    public readonly struct MicroDebuggerStopwatch
    {
        private static readonly double s_timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private readonly long _startTimestamp;

        private MicroDebuggerStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        internal static MicroDebuggerStopwatch StartNew()
            => new MicroDebuggerStopwatch(GetTimestamp());

        internal static long GetTimestamp()
            => Stopwatch.GetTimestamp();

        internal static TimeSpan GetElapsedTime(long startTimestamp, long endTimestamp)
        {
            var timestampDelta = endTimestamp - startTimestamp;
            var ticks = (long)(s_timestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }

        internal TimeSpan GetElapsedTime()
            => GetElapsedTime(_startTimestamp, GetTimestamp());
    }
}

#endif