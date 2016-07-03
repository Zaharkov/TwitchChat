using Database;
using Database.Entities;
using PythonApi;
using RestClientHelper;

namespace TwitchChat.Code.Commands
{
    public static class MmrCommand
    {
        private static readonly string SteamId = Configuration.GetSetting("SteamID");
        private static readonly int Delay = int.Parse(Configuration.GetSetting("SteamMmrDelay"));

        public static string GetMmr(MessageEventArgs e)
        {
            if (!e.Mod && !e.Subscriber && !e.Broadcaster)
                return null;

            return GetMmr();
        }

        private static string GetMmr()
        {
            int? soloInt = null;
            int? partyInt = null;

            var soloDb = SqLiteClient.GetNotExpiredToken(AccessTokenType.SoloMmr);
            if (!string.IsNullOrEmpty(soloDb))
            {
                int tryParse;

                if (int.TryParse(soloDb, out tryParse))
                    soloInt = tryParse;
            }

            var partyDb = SqLiteClient.GetNotExpiredToken(AccessTokenType.PartyMmr);
            if (!string.IsNullOrEmpty(partyDb))
            {
                int tryParse;

                if (int.TryParse(partyDb, out tryParse))
                    partyInt = tryParse;
            }

            if (!soloInt.HasValue || !partyInt.HasValue)
                GetMmr(out soloInt, out partyInt);

            return BuildString(soloInt, partyInt);
        }

        public static string MmrUpdate(MessageEventArgs e)
        {
            if (!e.Broadcaster)
                return null;

            int? solo;
            int? party;
            GetMmr(out solo, out party);

            return BuildString(solo, party);
        }

        private static void GetMmr(out int? solo, out int? party)
        {
            var mmr = PythonApiClient.GetMmr(SteamId);

            solo = mmr.SoloMmr.HasValue && mmr.SoloMmr.Value > 0 ? mmr.SoloMmr : null;
            party = mmr.PartyMmr.HasValue && mmr.PartyMmr.Value > 0 ? mmr.PartyMmr : null;

            SqLiteClient.AddToken(AccessTokenType.SoloMmr, solo?.ToString() ?? "0", Delay);
            SqLiteClient.AddToken(AccessTokenType.PartyMmr, party?.ToString() ?? "0", Delay);
        }

        private static string BuildString(int? soloInt, int? partyInt)
        {
            var solo = soloInt.HasValue && soloInt.Value > 0 ? $"Одиночный рейтинг равен: {soloInt.Value}" : "Одиночный рейтинг не доступен";
            var party = partyInt.HasValue && partyInt.Value > 0 ? $"Групповой рейтинг равен: {partyInt.Value}" : "Групповой рейтинг не доступен";

            return $"БОТ: {solo}. {party}";
        }
    }
}
