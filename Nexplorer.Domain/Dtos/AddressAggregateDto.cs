using System;
using Nexplorer.Domain.Enums;
using ProtoBuf;

namespace Nexplorer.Domain.Dtos
{
    [ProtoContract]
    public class AddressAggregateDto
    {
        [ProtoMember(1)]
        public int AddressId { get; set; }

        [ProtoMember(2)]
        public int LastBlockHeight { get; set; }

        [ProtoMember(3)]
        public double Balance { get; set; }

        [ProtoMember(4)]
        public double ReceivedAmount { get; set; }

        [ProtoMember(5)]
        public int ReceivedCount { get; set; }

        [ProtoMember(6)]
        public double SentAmount { get; set; }

        [ProtoMember(7)]
        public int SentCount { get; set; }

        public void ModifyAggregateProperties(TransactionType txType, double amount, int lastBlockHeight)
        {
            switch (txType)
            {
                case TransactionType.Input:
                    SentAmount = Math.Round(SentAmount + amount, 8);
                    SentCount++;
                    break;
                case TransactionType.Output:
                    ReceivedAmount = Math.Round(ReceivedAmount + amount, 8);
                    ReceivedCount++;
                    break;
            }

            Balance = Math.Round(ReceivedAmount - SentAmount, 8);

            if (lastBlockHeight > LastBlockHeight)
                LastBlockHeight = lastBlockHeight;
        }
    }
}