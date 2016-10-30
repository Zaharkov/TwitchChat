using Domain.Models;
using Domain.Repositories;
using DotaClient;
using Configuration;
using Configuration.Entities;

namespace TwitchChat.Code.Commands
{
    public static class MmrCommand
    {
        private static readonly Steam Steam = ConfigHolder.Configs.Steam;

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

            return SendMessage.GetMessage(BuildString(soloInt, partyInt));
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

            AccessTokenRepository.Instance.AddToken(AccessTokenType.SoloMmr, solo?.ToString() ?? "0", Steam.Params.MmrDelay);
            AccessTokenRepository.Instance.AddToken(AccessTokenType.PartyMmr, party?.ToString() ?? "0", Steam.Params.MmrDelay);
        }

        private static string BuildString(int? soloInt, int? partyInt)
        {
            var solo = soloInt.HasValue && soloInt.Value > 0 ? string.Format(Steam.Texts.Solo, soloInt.Value) : Steam.Texts.NoSolo;
            var party = partyInt.HasValue && partyInt.Value > 0 ? string.Format(Steam.Texts.Solo, partyInt.Value) : Steam.Texts.NoParty;

            return $"{solo}. {party}";
        }
    }
}
