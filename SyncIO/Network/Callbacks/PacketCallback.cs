using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network.Callbacks {
    internal class PacketCallback<SenderType>  {

        public static PacketCallback<SenderTypeCreate> Create<SenderTypeCreate, T>(Action<SenderTypeCreate, T> handler) 
            where T : class, IPacket {
            return new InnerPacketCallback<SenderTypeCreate, T>(handler);
        }

        public virtual void Raise(SenderType t, IPacket packet) {
        }

        public virtual Type Type { get; protected set; }
        public virtual bool IsType(Type t) {
            return false;
        }
    }
    internal class InnerPacketCallback<ST, RT> : PacketCallback<ST>
        where RT : class, IPacket  {

        private Action<ST, RT> Callback;
        public override Type Type {get; protected set; }
        public InnerPacketCallback(Action<ST, RT> cb) {
            Callback = cb;
            Type = typeof(RT);
        }

        public override void Raise(ST t, IPacket packet) {
            var pass = packet as RT;
            Callback(t, pass);
        }

        public override bool IsType(Type t) {
            return Type == t;
        }
    }

}
