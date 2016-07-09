using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Database;
using PythonApi.Entities;
using RestClientHelper;

namespace PythonApi
{
    /// <summary>
    /// Да да...я знаю что это полный пиздец
    /// Однако попытки прикрутить IronPython оказались тщетными 
    /// Толи библиотеки его слишком слабые, толи он несовместим с клиентами стима и доты
    /// </summary>
    public static class PythonApiClient
    {
        private static readonly string SteamUser = Configuration.GetSetting("SteamUser");
        private static readonly string SteamPass = Configuration.GetSetting("SteamPass");

        private static string Execute(string command)
        {
            if(File.Exists("command.py"))
                File.Delete("command.py");

            File.AppendAllText("command.py", command);

            try
            {
                var start = new ProcessStartInfo
                {
                    FileName = @"Python27\python.exe",
                    Arguments = "command.py",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(start))
                {
                    if(process == null)
                        throw new Exception("cannot start process");

                    using (var reader = process.StandardError)
                    {
                        var error = reader.ReadToEnd();

                        if(!string.IsNullOrEmpty(error))
                            throw new Exception(error);
                    }

                    using (var reader = process.StandardOutput)
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                SqLiteClient.LogException("PythonApi error", ex);
                Debug.WriteLine($"PythonApi error: {ex.Message}");
                return null;
            }
            
        }

        public static Mmr GetMmr(string steamId)
        {
            var command = $@"

from steam import SteamClient
from steam.enums import EResult
from steam.enums.emsg import EMsg
from dota2 import Dota2Client
import logging

profile_result = ''

logging.basicConfig(format='[%(asctime)s] %(levelname)s %(name)s: %(message)s', level=logging.CRITICAL)

logOnDetails = {{
    'username': '{SteamUser}',
    'password': '{SteamPass}',
}}

client = SteamClient()

@client.on('error')
def print_error(result):
    raise EResult(result)

client.login(**logOnDetails)
client.wait_event(EMsg.ClientAccountInfo, 10)
		
dota = Dota2Client(client)

@dota.on('done')
def done():
    done = 'done';

@dota.on('ready')
def ready():
    jobid = dota.request_profile_card({steamId})
    resp = dota.wait_event(jobid)
    global profile_result
    profile_result = resp
    dota.emit('done')


dota.on('done', done)
dota.launch()
dota.wait_event('done', 30)
print profile_result
";

            var result = Execute(command);

            if (result == null)
                return new Mmr();

            var solo = Regex.Match(result, @"stat_id: k_eStat_SoloRank.. *stat_score: (\d+)", RegexOptions.Singleline);
            var party = Regex.Match(result, @"stat_id: k_eStat_PartyRank.. *stat_score: (\d+)", RegexOptions.Singleline);

            int? soloInt = null;
            int? partyInt = null;

            if (solo.Success && solo.Groups.Count > 1 && !string.IsNullOrEmpty(solo.Groups[1].Value))
            {
                int tryParse;
                if (int.TryParse(solo.Groups[1].Value, out tryParse))
                    soloInt = tryParse;
            }

            if (party.Success && party.Groups.Count > 1 && !string.IsNullOrEmpty(party.Groups[1].Value))
            {
                int tryParse;
                if (int.TryParse(party.Groups[1].Value, out tryParse))
                    partyInt = tryParse;
            }

            return new Mmr
            {
                SoloMmr = soloInt,
                PartyMmr = partyInt
            };
        }
    }
}
