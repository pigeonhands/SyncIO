namespace SyncIO.Network.Callbacks
{
    using System;

    using SyncIO.Transport.Packets;

    internal class PacketCallback<SenderType>
    {
        public static PacketCallback<SenderType> Create<T>(Action<SenderType, T> handler)
            where T : class, IPacket
        {
            return new InnerPacketCallback<SenderType, T>(handler);
        }

        public virtual void Raise(SenderType t, IPacket packet)
        {
        }

        public virtual Type Type { get; protected set; }

        public virtual bool IsType(Type t)
        {
            return false;
        }
    }
    internal class InnerPacketCallback<ST, RT> : PacketCallback<ST>
        where RT : class, IPacket
    {
        private readonly Action<ST, RT> _callback;

        public override Type Type { get; protected set; }

        public InnerPacketCallback(Action<ST, RT> cb)
        {
            _callback = cb;
            Type = typeof(RT);
        }

        public override void Raise(ST t, IPacket packet)
        {
            var pass = packet as RT;
            _callback(t, pass);
        }

        public override bool IsType(Type t)
        {
            return Type == t;
        }
    }
}