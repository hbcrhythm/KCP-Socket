﻿using System;

namespace SGF.Network.KCP
{
    public class KCP
    {
        public const int IKCP_RTO_NDL = 30;  // no delay min rto
        public const int IKCP_RTO_MIN = 100; // normal min rto
        public const int IKCP_RTO_DEF = 200;
        public const int IKCP_RTO_MAX = 60000;
        public const int IKCP_CMD_PUSH = 81; // cmd: push data
        public const int IKCP_CMD_ACK = 82; // cmd: ack
        public const int IKCP_CMD_WASK = 83; // cmd: window probe (ask)
        public const int IKCP_CMD_WINS = 84; // cmd: window size (tell)
        public const int IKCP_ASK_SEND = 1;  // need to send IKCP_CMD_WASK
        public const int IKCP_ASK_TELL = 2;  // need to send IKCP_CMD_WINS
        public const int IKCP_WND_SND = 32;
        public const int IKCP_WND_RCV = 32;
        public const int IKCP_MTU_DEF = 1400;
        public const int IKCP_ACK_FAST = 3;
        public const int IKCP_INTERVAL = 100;
        public const int IKCP_OVERHEAD = 24;	//kcp包的包头大小
        public const int IKCP_DEADLINK = 10;
        public const int IKCP_THRESH_INIT = 2;
        public const int IKCP_THRESH_MIN = 2;
        public const int IKCP_PROBE_INIT = 7000;   // 7 secs to probe window size
        public const int IKCP_PROBE_LIMIT = 120000; // up to 120 secs to probe window


        // encode 8 bits unsigned int
        public static int ikcp_encode8u(byte[] p, int offset, byte c)
        {
            p[0 + offset] = c;
            return 1;
        }

        // decode 8 bits unsigned int
        public static int ikcp_decode8u(byte[] p, int offset, ref byte c)
        {
            c = p[0 + offset];
            return 1;
        }

        /* encode 16 bits unsigned int (lsb) */
        public static int ikcp_encode16u(byte[] p, int offset, UInt16 w)
        {
            p[0 + offset] = (byte)(w >> 0);
            p[1 + offset] = (byte)(w >> 8);
            return 2;
        }

        /* decode 16 bits unsigned int (lsb) */
        public static int ikcp_decode16u(byte[] p, int offset, ref UInt16 c)
        {
            UInt16 result = 0;
            result |= (UInt16)p[0 + offset];
            result |= (UInt16)(p[1 + offset] << 8);
            c = result;
            return 2;
        }

        /* encode 32 bits unsigned int (lsb) */
        public static int ikcp_encode32u(byte[] p, int offset, UInt32 l)
        {
            p[0 + offset] = (byte)(l >> 0);
            p[1 + offset] = (byte)(l >> 8);
            p[2 + offset] = (byte)(l >> 16);
            p[3 + offset] = (byte)(l >> 24);
            return 4;
        }

        /* decode 32 bits unsigned int (lsb) */
        public static int ikcp_decode32u(byte[] p, int offset, ref UInt32 c)
        {
            UInt32 result = 0;
            result |= (UInt32)p[0 + offset];
            result |= (UInt32)(p[1 + offset] << 8);
            result |= (UInt32)(p[2 + offset] << 16);
            result |= (UInt32)(p[3 + offset] << 24);
            c = result;
            return 4;
        }

        public static byte[] slice(byte[] p, int start, int stop)
        {
            var bytes = new byte[stop - start];
            Array.Copy(p, start, bytes, 0, bytes.Length);
            return bytes;
        }

        public static T[] slice<T>(T[] p, int start, int stop)
        {
            var arr = new T[stop - start];
            var index = 0;
            for (var i = start; i < stop; i++)
            {
                arr[index] = p[i];
                index++;
            }

            return arr;
        }

        public static byte[] append(byte[] p, byte c)
        {
            var bytes = new byte[p.Length + 1];
            Array.Copy(p, bytes, p.Length);
            bytes[p.Length] = c;
            return bytes;
        }

        public static T[] append<T>(T[] p, T c)
        {
            var arr = new T[p.Length + 1];
            for (var i = 0; i < p.Length; i++)
                arr[i] = p[i];
            arr[p.Length] = c;
            return arr;
        }

        public static T[] append<T>(T[] p, T[] cs)
        {
            var arr = new T[p.Length + cs.Length];
            for (var i = 0; i < p.Length; i++)
                arr[i] = p[i];
            for (var i = 0; i < cs.Length; i++)
                arr[p.Length + i] = cs[i];
            return arr;
        }

        static UInt32 _imin_(UInt32 a, UInt32 b)
        {
            return a <= b ? a : b;
        }

        static UInt32 _imax_(UInt32 a, UInt32 b)
        {
            return a >= b ? a : b;
        }

        static UInt32 _ibound_(UInt32 lower, UInt32 middle, UInt32 upper)
        {
            return _imin_(_imax_(lower, middle), upper);
        }

        static Int32 _itimediff(UInt32 later, UInt32 earlier)
        {
            return ((Int32)(later - earlier));
        }

        // KCP Segment Definition
        internal class Segment
        {
            internal UInt32 conv = 0;
            internal UInt32 cmd = 0;
            internal UInt32 frg = 0;
            internal UInt32 wnd = 0;
            internal UInt32 ts = 0;
            internal UInt32 sn = 0;
            internal UInt32 una = 0;
            internal UInt32 resendts = 0;
            internal UInt32 rto = 0;
            internal UInt32 fastack = 0;
            internal UInt32 xmit = 0;
            internal byte[] data;

            internal Segment(int size)
            {
                this.data = new byte[size];
            }

            // encode a segment into buffer
            internal int encode(byte[] ptr, int offset)
            {

                var offset_ = offset;

                offset += ikcp_encode32u(ptr, offset, conv);
                offset += ikcp_encode8u(ptr, offset, (byte)cmd);
                offset += ikcp_encode8u(ptr, offset, (byte)frg);
                offset += ikcp_encode16u(ptr, offset, (UInt16)wnd);
                offset += ikcp_encode32u(ptr, offset, ts);
                offset += ikcp_encode32u(ptr, offset, sn);
                offset += ikcp_encode32u(ptr, offset, una);
                offset += ikcp_encode32u(ptr, offset, (UInt32)data.Length);

                return offset - offset_;
            }
        }

        // kcp members.
        UInt32 conv; UInt32 mtu; UInt32 mss; UInt32 state;
        UInt32 snd_una; UInt32 snd_nxt; UInt32 rcv_nxt;
        UInt32 ts_recent; UInt32 ts_lastack; UInt32 ssthresh;
        UInt32 rx_rttval; UInt32 rx_srtt; UInt32 rx_rto; UInt32 rx_minrto;
        UInt32 snd_wnd; UInt32 rcv_wnd; UInt32 rmt_wnd; UInt32 cwnd; UInt32 probe;
        UInt32 current; UInt32 interval; UInt32 ts_flush; UInt32 xmit;
        UInt32 nodelay; UInt32 updated;
        UInt32 ts_probe; UInt32 probe_wait;
        UInt32 dead_link; UInt32 incr;

        Segment[] snd_queue = new Segment[0];
        Segment[] rcv_queue = new Segment[0];
        Segment[] snd_buf = new Segment[0];
        Segment[] rcv_buf = new Segment[0];

        UInt32[] acklist = new UInt32[0];

        byte[] buffer;
        Int32 fastresend;
        Int32 nocwnd;
        Int32 logmask;
        // buffer, size
        Action<byte[], int> output;

        // create a new kcp control object, 'conv' must equal in two endpoint
        // from the same connection.
        public KCP(UInt32 conv_, Action<byte[], int> output_)
        {
            conv = conv_;
            snd_wnd = IKCP_WND_SND;
            rcv_wnd = IKCP_WND_RCV;
            rmt_wnd = IKCP_WND_RCV;
            mtu = IKCP_MTU_DEF;
            mss = mtu - IKCP_OVERHEAD;

            rx_rto = IKCP_RTO_DEF;
            rx_minrto = IKCP_RTO_MIN;
            interval = IKCP_INTERVAL;
            ts_flush = IKCP_INTERVAL;
            ssthresh = IKCP_THRESH_INIT;
            dead_link = IKCP_DEADLINK;
            buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
            output = output_;
        }

        public void Dispose()
        {
            output = null;
        }

        // check the size of next message in the recv queue
        public int PeekSize()
        {

            if (0 == rcv_queue.Length) return -1;

            var seq = rcv_queue[0];

            if (0 == seq.frg) return seq.data.Length;

            if (rcv_queue.Length < seq.frg + 1) return -1;

            int length = 0;

            foreach (var item in rcv_queue)
            {
                length += item.data.Length;
                if (0 == item.frg)
                    break;
            }

            return length;
        }

        // user/upper level recv: returns size, returns below zero for EAGAIN
        public int Recv(byte[] buffer)
        {

            if (0 == rcv_queue.Length) return -1;

            var peekSize = PeekSize();
            if (0 > peekSize) return -2;

            if (peekSize > buffer.Length) return -3;

			//fast_recover标识的意思是快速告知对端我又有窗口大小空出来了，因为在Input函数中有可能窗口会满了，
			//此时发送给对端的是窗口满消息，而在recv过后，因为取走了消息，可用接收窗口又变大了，此时需要快速告知对端可以继续发消息了。
            var fast_recover = false;
            if (rcv_queue.Length >= rcv_wnd) fast_recover = true;

            // merge fragment.
            var count = 0;
            var n = 0;
            foreach (var seg in rcv_queue)
            {
                Array.Copy(seg.data, 0, buffer, n, seg.data.Length);
                n += seg.data.Length;
                count++;
				//0表示最后一个分片
                if (0 == seg.frg) break;
            }

            if (0 < count)
            {
				//把队列剩下的Segment重新调整索引 
                rcv_queue = slice<Segment>(rcv_queue, count, rcv_queue.Length);
            }

            // move available data from rcv_buf -> rcv_queue
            count = 0;
            foreach (var seg in rcv_buf)
            {
                if (seg.sn == rcv_nxt && rcv_queue.Length < rcv_wnd)
                {
                    rcv_queue = append<Segment>(rcv_queue, seg);
                    rcv_nxt++;
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (0 < count) rcv_buf = slice<Segment>(rcv_buf, count, rcv_buf.Length);

            // fast recover
            if (rcv_queue.Length < rcv_wnd && fast_recover)
            {
                // ready to send back IKCP_CMD_WINS in ikcp_flush
                // tell remote my window size
                probe |= IKCP_ASK_TELL;
            }

            return n;
        }

        // user/upper level send, returns below zero for error
        public int Send(byte[] buffer, int bufferSize)
        {

            if (0 == bufferSize) return -1;

            var count = 0;

            if (bufferSize < mss)
                count = 1;
            else
				//这里的count 这样写是因为要往上取整。
                count = (int)(bufferSize + mss - 1) / (int)mss;

            if (255 < count) return -2;

            if (0 == count) count = 1;

            var offset = 0;

            for (var i = 0; i < count; i++)
            {
                var size = 0;
                if (bufferSize > mss)
                    size = (int)mss;
                else
                    size = bufferSize - offset;

                var seg = new Segment(size);
                Array.Copy(buffer, offset, seg.data, 0, size);
                offset += size;
				//frg 是从大到小的， 最后一个是0哦。
				//这里只实现了消息模式，并没有实现流模式、。
                seg.frg = (UInt32)(count - i - 1);
                snd_queue = append<Segment>(snd_queue, seg);
            }

            return 0;
        }

        // update ack.
        void update_ack(Int32 rtt)
        {
            if (0 == rx_srtt)
            {
                rx_srtt = (UInt32)rtt;
                rx_rttval = (UInt32)rtt / 2;
            }
            else
            {
                Int32 delta = (Int32)((UInt32)rtt - rx_srtt);
                if (0 > delta) delta = -delta;

                rx_rttval = (3 * rx_rttval + (uint)delta) / 4;
                rx_srtt = (UInt32)((7 * rx_srtt + rtt) / 8);
                if (rx_srtt < 1) rx_srtt = 1;
            }

			//根据 rx_srtt rx_rttval 计算出rx_tro(重传超时时间).
            var rto = (int)(rx_srtt + _imax_(1, 4 * rx_rttval));
            rx_rto = _ibound_(rx_minrto, (UInt32)rto, IKCP_RTO_MAX);
        }

		//更新第一个未确认的包。就是对端还没告诉我们这个包他们收到了。然后我们需要更新 第一个未收到确认的包的字段(send_una)
        void shrink_buf()
        {
            if (snd_buf.Length > 0)
                snd_una = snd_buf[0].sn;
            else
                snd_una = snd_nxt;
        }

		//收到act包之后需要删除发送换从区中与ack包中的 发送序列号 sn 相同的kcp包。
        void parse_ack(UInt32 sn)
        {

            if (_itimediff(sn, snd_una) < 0 || _itimediff(sn, snd_nxt) >= 0) return;

            var index = 0;
            foreach (var seg in snd_buf)
            {
                if (sn == seg.sn)
                {
                    snd_buf = append<Segment>(slice<Segment>(snd_buf, 0, index), slice<Segment>(snd_buf, index + 1, snd_buf.Length));
                    break;
                }
                else
                {
                    seg.fastack++;
                }

                index++;
            }
        }

		//根据接受到的una 表示对端 希望接收到的下一个kcp包的序号，也就是说对端已经收到了所有小于una的kcp包了。
		//那么既然对端已经收到所有小于una的包了，那么就需要把发送缓冲区中小于una的包删掉。
        void parse_una(UInt32 una)
        {
            var count = 0;
            foreach (var seg in snd_buf)
            {
                if (_itimediff(una, seg.sn) > 0)
                    count++;
                else
                    break;
            }

            if (0 < count) snd_buf = slice<Segment>(snd_buf, count, snd_buf.Length);
        }

		//保存ack
        void ack_push(UInt32 sn, UInt32 ts)
        {
            acklist = append<UInt32>(acklist, new UInt32[2] { sn, ts });
        }

		//获得acklist中的sn和ts 这个这个p因为在外面/2 所以在这里要*2 
        void ack_get(int p, ref UInt32 sn, ref UInt32 ts)
        {
            sn = acklist[p * 2 + 0];
            ts = acklist[p * 2 + 1];
        }

        void parse_data(Segment newseg)
        {
            var sn = newseg.sn;
            if (_itimediff(sn, rcv_nxt + rcv_wnd) >= 0 || _itimediff(sn, rcv_nxt) < 0) return;

            var n = rcv_buf.Length - 1;
            var after_idx = -1;
            var repeat = false;
            for (var i = n; i >= 0; i--)
            {
                var seg = rcv_buf[i];
                if (seg.sn == sn)
                {
                    repeat = true;
                    break;
                }

                if (_itimediff(sn, seg.sn) > 0)
                {
                    after_idx = i;
                    break;
                }
            }

			//根据repeat 重新调整rev_buf数据
            if (!repeat)
            {
                if (after_idx == -1)
                    rcv_buf = append<Segment>(new Segment[1] { newseg }, rcv_buf);
                else
                    rcv_buf = append<Segment>(slice<Segment>(rcv_buf, 0, after_idx + 1), append<Segment>(new Segment[1] { newseg }, slice<Segment>(rcv_buf, after_idx + 1, rcv_buf.Length)));
            }

            // move available data from rcv_buf -> rcv_queue
			//这里是判断接收到的segment.sn是不是等于接收端下一个要接受的序号，是的话那么需要把这个数据从buf移动到queue里面，并且更新rcv_next
            var count = 0;
            foreach (var seg in rcv_buf)
            {
                if (seg.sn == rcv_nxt && rcv_queue.Length < rcv_wnd)
                {
                    rcv_queue = append<Segment>(rcv_queue, seg);
                    rcv_nxt++;
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (0 < count)
            {
                rcv_buf = slice<Segment>(rcv_buf, count, rcv_buf.Length);
            }
        }

        // when you received a low level packet (eg. UDP packet), call it
        public int Input(byte[] data)
        {

            var s_una = snd_una;
            if (data.Length < IKCP_OVERHEAD) return 0;

            var offset = 0;

            while (true)
            {
                UInt32 ts = 0;
                UInt32 sn = 0;
                UInt32 length = 0;
                UInt32 una = 0;
                UInt32 conv_ = 0;

                UInt16 wnd = 0;

                byte cmd = 0;
                byte frg = 0;

                if (data.Length - offset < IKCP_OVERHEAD) break;

                offset += ikcp_decode32u(data, offset, ref conv_);

                if (conv != conv_) return -1;

                offset += ikcp_decode8u(data, offset, ref cmd);
                offset += ikcp_decode8u(data, offset, ref frg);
                offset += ikcp_decode16u(data, offset, ref wnd);
                offset += ikcp_decode32u(data, offset, ref ts);
                offset += ikcp_decode32u(data, offset, ref sn);
                offset += ikcp_decode32u(data, offset, ref una);
                offset += ikcp_decode32u(data, offset, ref length);

                if (data.Length - offset < length) return -2;

                switch (cmd)
                {
                    case IKCP_CMD_PUSH:
                    case IKCP_CMD_ACK:
                    case IKCP_CMD_WASK:
                    case IKCP_CMD_WINS:
                        break;
                    default:
                        return -3;
                }

                rmt_wnd = (UInt32)wnd;
                parse_una(una);
                shrink_buf();

                if (IKCP_CMD_ACK == cmd)
                {
                    if (_itimediff(current, ts) >= 0)
                    {
                        update_ack(_itimediff(current, ts));
                    }
                    parse_ack(sn);
                    shrink_buf();
                }
                else if (IKCP_CMD_PUSH == cmd)
                {
					//判断这个segment的序号是否在接受窗口内。  /* 这里也缺了一些实现。
                    if (_itimediff(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        ack_push(sn, ts);
                        if (_itimediff(sn, rcv_nxt) >= 0)
                        {
                            var seg = new Segment((int)length);
                            seg.conv = conv_;
                            seg.cmd = (UInt32)cmd;
                            seg.frg = (UInt32)frg;
                            seg.wnd = (UInt32)wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;

                            if (length > 0) Array.Copy(data, offset, seg.data, 0, length);

                            parse_data(seg);
                        }
                    }
                }
                else if (IKCP_CMD_WASK == cmd)
                {
                    // ready to send back IKCP_CMD_WINS in Ikcp_flush
                    // tell remote my window size
                    probe |= IKCP_ASK_TELL;
                }
                else if (IKCP_CMD_WINS == cmd)
                {
                    // do nothing
                }
                else
                {
                    return -3;
                }

                offset += (int)length;
            }

            if (_itimediff(snd_una, s_una) > 0)
            {
                if (cwnd < rmt_wnd)
                {
                    var mss_ = mss;
                    if (cwnd < ssthresh)
                    {
                        cwnd++;
                        incr += mss_;
                    }
                    else
                    {
                        if (incr < mss_)
                        {
                            incr = mss_;
                        }
                        incr += (mss_ * mss_) / incr + (mss_ / 16);
                        if ((cwnd + 1) * mss_ <= incr) cwnd++;
                    }
                    if (cwnd > rmt_wnd)
                    {
                        cwnd = rmt_wnd;
                        incr = rmt_wnd * mss_;
                    }
                }
            }

            return 0;
        }

		//获得剩余接受窗口大小
        Int32 wnd_unused()
        {
            if (rcv_queue.Length < rcv_wnd)
                return (Int32)(int)rcv_wnd - rcv_queue.Length;
            return 0;
        }

        // flush pending data
        void flush()
        {
            var current_ = current;
            var buffer_ = buffer;
            var change = 0;
            var lost = 0;

            if (0 == updated) return;

            var seg = new Segment(0);
            seg.conv = conv;
            seg.cmd = IKCP_CMD_ACK;
            seg.wnd = (UInt32)wnd_unused();
            seg.una = rcv_nxt;

			//发送ack
            // flush acknowledges
            var count = acklist.Length / 2;
            var offset = 0;
            for (var i = 0; i < count; i++)
            {
                if (offset + IKCP_OVERHEAD > mtu)
                {
					//检测如果超过mtu就将小于mtu的包发送出去。
                    output(buffer, offset);
                    //Array.Clear(buffer, 0, offset);
                    offset = 0;
                }
                ack_get(i, ref seg.sn, ref seg.ts);
                offset += seg.encode(buffer, offset);
            }
            acklist = new UInt32[0];

            // probe window size (if remote window size equals zero)
            if (0 == rmt_wnd)
            {
                if (0 == probe_wait)
                {
                    probe_wait = IKCP_PROBE_INIT;
                    ts_probe = current + probe_wait;
                }
                else
                {
                    if (_itimediff(current, ts_probe) >= 0)
                    {
						//探查窗口的时间会逐渐被大，但是不会超过限制
                        if (probe_wait < IKCP_PROBE_INIT)
                            probe_wait = IKCP_PROBE_INIT;
                        probe_wait += probe_wait / 2;
                        if (probe_wait > IKCP_PROBE_LIMIT)
                            probe_wait = IKCP_PROBE_LIMIT;
                        ts_probe = current + probe_wait;
                        probe |= IKCP_ASK_SEND;
                    }
                }
            }
            else
            {
                ts_probe = 0;
                probe_wait = 0;
            }

			//发送探查窗口
            // flush window probing commands
            if ((probe & IKCP_ASK_SEND) != 0)
            {
                seg.cmd = IKCP_CMD_WASK;
                if (offset + IKCP_OVERHEAD > (int)mtu)
                {
                    output(buffer, offset);
                    //Array.Clear(buffer, 0, offset);
                    offset = 0;
                }
                offset += seg.encode(buffer, offset);
            }

            probe = 0;

            // calculate window size
            var cwnd_ = _imin_(snd_wnd, rmt_wnd);
            if (0 == nocwnd)
                cwnd_ = _imin_(cwnd, cwnd_);

            count = 0;
            for (var k = 0; k < snd_queue.Length; k++)
            {
                if (_itimediff(snd_nxt, snd_una + cwnd_) >= 0) break;

                var newseg = snd_queue[k];
                newseg.conv = conv;
                newseg.cmd = IKCP_CMD_PUSH;
                newseg.wnd = seg.wnd;
                newseg.ts = current_;
                newseg.sn = snd_nxt;
                newseg.una = rcv_nxt;
                newseg.resendts = current_;
                newseg.rto = rx_rto;
                newseg.fastack = 0;
                newseg.xmit = 0;
                snd_buf = append<Segment>(snd_buf, newseg);
                snd_nxt++;
                count++;
            }

            if (0 < count)
            {
                snd_queue = slice<Segment>(snd_queue, count, snd_queue.Length);
            }

            // calculate resent
            var resent = (UInt32)fastresend;
            if (fastresend <= 0) resent = 0xffffffff;
            var rtomin = rx_rto >> 3;
            if (nodelay != 0) rtomin = 0;

            // flush data segments
            foreach (var segment in snd_buf)
            {
                var needsend = false;
                var debug = _itimediff(current_, segment.resendts);
                if (0 == segment.xmit)	//新的segment
                {
                    needsend = true;
                    segment.xmit++;
                    segment.rto = rx_rto;
                    segment.resendts = current_ + segment.rto + rtomin;
                }
                else if (_itimediff(current_, segment.resendts) >= 0) //已经发送过的segment 这里是重传
                {
                    needsend = true;
                    segment.xmit++;
                    xmit++;
                    if (0 == nodelay)
                        segment.rto += rx_rto;
                    else
                        segment.rto += rx_rto / 2;
                    segment.resendts = current_ + segment.rto;
                    lost = 1;
                }
                else if (segment.fastack >= resent)	//收到ack时候会计算该分片被累计的跳过次数，如果大于 配置的 出发快速重传的重复ack个数，那么就马上重传
                {
                    needsend = true;
                    segment.xmit++;
                    segment.fastack = 0;
                    segment.resendts = current_ + segment.rto;
                    change++;
                }

                if (needsend)
                {
                    segment.ts = current_;
                    segment.wnd = seg.wnd;
                    segment.una = rcv_nxt;

                    var need = IKCP_OVERHEAD + segment.data.Length;
                    if (offset + need >= mtu)
                    {
                        output(buffer, offset);
                        //Array.Clear(buffer, 0, offset);
                        offset = 0;
                    }

                    offset += segment.encode(buffer, offset);
                    if (segment.data.Length > 0)
                    {
                        Array.Copy(segment.data, 0, buffer, offset, segment.data.Length);
                        offset += segment.data.Length;
                    }

                    if (segment.xmit >= dead_link)
                    {
                        state = 0;
                    }
                }
            }

            // flash remain segments
            if (offset > 0)
            {
                output(buffer, offset);
                //Array.Clear(buffer, 0, offset);
                offset = 0;
            }

			//在发生快速重传的时候，会将慢启动阈值调整为当前发送窗口的一半，并把拥塞窗口大小调整为kcp.ssthresh + resent，
			//resent是触发快速重传的丢包的次数，resent的值代表的意思在被弄丢的包后面收到了resent个数的包的ack。这样调整后kcp就进入了拥塞控制状态。
            // update ssthresh
            if (change != 0)
            {
                var inflight = snd_nxt - snd_una;
                ssthresh = inflight / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                    ssthresh = IKCP_THRESH_MIN;
                cwnd = ssthresh + resent;
                incr = cwnd * mss;
            }

            if (lost != 0)
            {
                ssthresh = cwnd / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                    ssthresh = IKCP_THRESH_MIN;
                cwnd = 1;
                incr = mss;
            }

            if (cwnd < 1)
            {
                cwnd = 1;
                incr = mss;
            }
        }

        // update state (call it repeatedly, every 10ms-100ms), or you can ask
        // ikcp_check when to call it again (without ikcp_input/_send calling).
        // 'current' - current timestamp in millisec.
        public void Update(UInt32 current_)
        {

            current = current_;

            if (0 == updated)
            {
                updated = 1;
                ts_flush = current;
            }

            var slap = _itimediff(current, ts_flush);

            if (slap >= 10000 || slap < -10000)
            {
                ts_flush = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                ts_flush += interval;
                if (_itimediff(current, ts_flush) >= 0)
                    ts_flush = current + interval;
                flush();
            }
        }

        // Determine when should you invoke ikcp_update:
        // returns when you should invoke ikcp_update in millisec, if there
        // is no ikcp_input/_send calling. you can call ikcp_update in that
        // time, instead of call update repeatly.
        // Important to reduce unnacessary ikcp_update invoking. use it to
        // schedule ikcp_update (eg. implementing an epoll-like mechanism,
        // or optimize ikcp_update when handling massive kcp connections)
        public UInt32 Check(UInt32 current_)
        {

            if (0 == updated) return current_;

            var ts_flush_ = ts_flush;
            var tm_flush_ = 0x7fffffff;
            var tm_packet = 0x7fffffff;
            var minimal = 0;

            if (_itimediff(current_, ts_flush_) >= 10000 || _itimediff(current_, ts_flush_) < -10000)
            {
                ts_flush_ = current_;
            }

            if (_itimediff(current_, ts_flush_) >= 0) return current_;

            tm_flush_ = (int)_itimediff(ts_flush_, current_);

            foreach (var seg in snd_buf)
            {
                var diff = _itimediff(seg.resendts, current_);
                if (diff <= 0) return current_;
                if (diff < tm_packet) tm_packet = (int)diff;
            }

            minimal = (int)tm_packet;
            if (tm_packet >= tm_flush_) minimal = (int)tm_flush_;
            if (minimal >= interval) minimal = (int)interval;

            return current_ + (UInt32)minimal;
        }

        // change MTU size, default is 1400
        public int SetMtu(Int32 mtu_)
        {
            if (mtu_ < 50 || mtu_ < (Int32)IKCP_OVERHEAD) return -1;

            var buffer_ = new byte[(mtu_ + IKCP_OVERHEAD) * 3];
            if (null == buffer_) return -2;

            mtu = (UInt32)mtu_;
            mss = mtu - IKCP_OVERHEAD;
            buffer = buffer_;
            return 0;
        }

        public int Interval(Int32 interval_)
        {
            if (interval_ > 5000)
            {
                interval_ = 5000;
            }
            else if (interval_ < 10)
            {
                interval_ = 10;
            }
            interval = (UInt32)interval_;
            return 0;
        }

        // fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        // nodelay: 0:disable(default), 1:enable
        // interval: internal update timer interval in millisec, default is 100ms
        // resend: 0:disable fast resend(default), 1:enable fast resend
        // nc: 0:normal congestion control(default), 1:disable congestion control
        public int NoDelay(int nodelay_, int interval_, int resend_, int nc_)
        {

            if (nodelay_ > 0)
            {
                nodelay = (UInt32)nodelay_;
                if (nodelay_ != 0)
                    rx_minrto = IKCP_RTO_NDL;
                else
                    rx_minrto = IKCP_RTO_MIN;
            }

            if (interval_ >= 0)
            {
                if (interval_ > 5000)
                {
                    interval_ = 5000;
                }
                else if (interval_ < 10)
                {
                    interval_ = 10;
                }
                interval = (UInt32)interval_;
            }

            if (resend_ >= 0) fastresend = resend_;

            if (nc_ >= 0) nocwnd = nc_;

            return 0;
        }

        // set maximum window size: sndwnd=32, rcvwnd=32 by default
        public int WndSize(int sndwnd, int rcvwnd)
        {
            if (sndwnd > 0)
                snd_wnd = (UInt32)sndwnd;

            if (rcvwnd > 0)
                rcv_wnd = (UInt32)rcvwnd;
            return 0;
        }

        // get how many packet is waiting to be sent
        public int WaitSnd()
        {
            return snd_buf.Length + snd_queue.Length;
        }
    }


}
