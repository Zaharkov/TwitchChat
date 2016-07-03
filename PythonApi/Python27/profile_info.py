from steam import SteamClient
from steam.enums import EResult
from steam.enums.emsg import EMsg
from dota2 import Dota2Client
import logging

profile_result = ""

logging.basicConfig(format='[%(asctime)s] %(levelname)s %(name)s: %(message)s', level=logging.CRITICAL)

logOnDetails = {
    'username': "antimozgg",
    'password': "89152577734",
}

client = SteamClient()

@client.on('error')
def print_error(result):
    print "Error:", EResult(result)
	
@client.on('auth_code_required')
def auth_code_prompt(is_2fa, code_mismatch):
    if is_2fa:
        code = raw_input("Enter 2FA Code: ")
        client.login(two_factor_code=code, **logOnDetails)
    else:
        code = raw_input("Enter Email Code: ")
        client.login(auth_code=code, **logOnDetails)

client.login(**logOnDetails)
client.wait_event(EMsg.ClientAccountInfo)
		
dota = Dota2Client(client)

@dota.on('done')
def done():
    done = "done";

@dota.on('ready')
def ready():
	jobid = dota.request_profile_card(116261058)
	resp = dota.wait_event(jobid)
	global profile_result 
	profile_result = resp
	dota.emit("done")
	
dota.on('done', done)
dota.launch()
dota.wait_event("done", 30)
print profile_result








