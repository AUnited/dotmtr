/// dotMTR is copyright 2010 Nate McKay (natemckay@gmail.com)
/// dotMTR is release to the public under version 2 of the GPL: http://www.gnu.org/licenses/gpl-2.0.txt

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace dotMTR
{
    /// <summary>
    /// nPingDef class
    /// </summary>
    public class DotPing
    {
        /// <summary>
        /// Destination IP
        /// </summary>
        public IPAddress destIP;

        /// <summary>
        /// Time to live value
        /// </summary>
        public int ttl;

        /// <summary>
        /// Remote IP
        /// </summary>
        public IPAddress respIP;

        /// <summary>
        /// Milliseconds
        /// </summary>
        public long msecs;


        /// <summary>
        /// Ping status (set initial state to unknown)
        /// </summary>
        public IPStatus result = IPStatus.Unknown;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DotPing()
        {

        }

        /// <summary>
        /// Overload constructor
        /// </summary>
        /// <param name="_destIP"></param>
        /// <param name="_ttl"></param>
        public DotPing(IPAddress _destIP)
        {
            this.destIP = _destIP;
            this.ttl = 30;
        }

        /// <summary>
        /// Overload constructor
        /// </summary>
        /// <param name="_destIP"></param>
        /// <param name="_ttl"></param>
        public DotPing(IPAddress _destIP, int _ttl)
        {
            this.destIP = _destIP;
            this.ttl = _ttl;
        }
    }


    public class DotHop
    {
        public int hop;
        public IPAddress destIP;
        public List<IPStatus> statuss = new List<IPStatus>();
        public List<int> ttls = new List<int>();
        public List<double?> tripTimes = new List<double?>();
        public List<DateTime> timeStamps = new List<DateTime>();

        public IPAddress hopIP;

        public string hopIPStr
        {
            get { return ((this.hopIP != null) ? this.hopIP.ToString() : "*"); }
        }

        public int sent
        {
            get { return this.statuss.Count; }
        }

        public int recvd
        {
            get { return this.statuss.FindAll(new System.Predicate<IPStatus>(new statusCheckDelegate(statusCheck))).Count; }
        }

        public IPStatus status
        {
            get { return this.statuss.Last(); }
            set { this.statuss.Add(value); }
        }

        public int ttl
        {
            get { return this.ttls.Last(); }
            set { this.ttls.Add(value); }
        }

        public double? tripTime
        {
            get { return this.tripTimes.Last(); }
            set { this.tripTimes.Add(value); }
        }

        public string tripTimeStr
        {
            get { return ((tripTime.HasValue) ? this.tripTime.Value.ToString("0.00ms") : "*"); }
        }

        public DateTime timeStamp
        {
            get { return this.timeStamps.Last(); }
            set { this.timeStamps.Add(value); }
        }

        public double? last
        {
            get { return this.tripTimes.Last(); }
        }

        public double? best
        {
            get { return this.tripTimes.Min(); }
        }

        public double? worst
        {
            get { return this.tripTimes.Max(); }
        }

        public double? average
        {
            get { return this.tripTimes.Average(); }
        }

        public DotHop()
        {
        }

        public DotHop(IPAddress _destIP, int _ttl)
        {
            this.hop = _ttl;
            this.destIP = _destIP;
            this.ttl = _ttl;
        }
        
        private delegate bool statusCheckDelegate(IPStatus _status);

        private bool statusCheck(IPStatus _status)
        {
            switch(_status)
            {
                case IPStatus.TtlExpired:
                    return true;
                case IPStatus.Success:
                    return true;
                case IPStatus.DestinationNetworkUnreachable:
                    return true;
                case IPStatus.DestinationHostUnreachable:
                    return true;
                case IPStatus.DestinationPortUnreachable:
                    return true;
                case IPStatus.TimedOut:
                    return false;
            }

            return false;
        }

        public bool IsLastHop
        {
            get
            {
                switch (this.status)
                {
                    case IPStatus.Success:
                        return true;
                    case IPStatus.DestinationNetworkUnreachable:
                        return true;
                    case IPStatus.DestinationHostUnreachable:
                        return true;
                    case IPStatus.DestinationPortUnreachable:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
