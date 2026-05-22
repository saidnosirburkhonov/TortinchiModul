namespace Tg_Bot_4_6_dars
{
    internal class TelegramBotClient
    {
        private string v;

        public TelegramBotClient(string v)
        {
            this.v = v;
        }

        internal void StartReceiving(Func<ITelegramBotClient, Update, CancellationToken, Task> handleUpdateAsync, Func<ITelegramBotClient, Exception, CancellationToken, Task> handleErrorAsync, ReceiverOptions receiverOptions, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}