using CommonHelper;
using Domain.Models;
using Domain.Repositories;
using DotaClient;

namespace TwitchChat.Code.Commands
{
    public static class MmrCommand
    {
        private static readonly int Delay = int.Parse(Configuration.GetSetting("SteamMmrDelay"));

        public static SendMessage GetMmr()
        {
            int? soloInt = null;
            int? partyInt = null;

            var soloDb = AccessTokenRepository.Instance.GetNotExpiredToken(AccessTokenType.SoloMmr);
            if (!string.IsNullOrEmpty(soloDb))
            {
                int tryParse;

                if (int.TryParse(soloDb, out tryParse))
                    soloInt = tryParse;
            }

            var partyDb = AccessTokenRepository.Instance.GetNotExpiredToken(AccessTokenType.PartyMmr);
            if (!string.IsNullOrEmpty(partyDb))
            {
                int tryParse;

                if (int.TryParse(partyDb, out tryParse))
                    partyInt = tryParse;
            }

            if (!soloInt.HasValue || !partyInt.HasValue)
                GetMmr(out soloInt, out partyInt);

            return SendMessage.GetMessage(BuildString(soloInt, soloInt));
        }

        public static SendMessage MmrUpdate()
        {
            int? solo;
            int? party;
            GetMmr(out solo, out party);

            return SendMessage.GetMessage(BuildString(solo, party));
        }

        private static void GetMmr(out int? solo, out int? party)
        {
            DotaClientApi.GetMmr(out solo, out party);

            solo = solo.HasValue && solo.Value > 0 ? solo : null;
            party = party.HasValue && party.Value > 0 ? party : null;

            AccessTokenRepository.Instance.AddToken(AccessTokenType.SoloMmr, solo?.ToString() ?? "0", Delay);
            AccessTokenRepository.Instance.AddToken(AccessTokenType.PartyMmr, party?.ToString() ?? "0", Delay);
        }

        private static string BuildString(int? soloInt, int? partyInt)
        {
            var solo = soloInt.HasValue && soloInt.Value > 0 ? $"Одиночный рейтинг равен: {soloInt.Value}" : "Одиночный рейтинг не доступен";
            var party = partyInt.HasValue && partyInt.Value > 0 ? $"Групповой рейтинг равен: {partyInt.Value}" : "Групповой рейтинг не доступен";

            return $"{solo}. {party}";
        }
    }
}
