import signal
import random
import time
import zmq

use_dummy_model = 0
use_cli = 0

if not use_dummy_model:
	print("Preparing semantic text similarity model")
	from ..external.semantic_text_similarity.semantic_text_similarity.models import WebBertSimilarity
	web_model = WebBertSimilarity(device='cpu', batch_size=10) #defaults to GPU prediction
else:
	print("Using dummy semantic text similarity model")

class SpellMod:
	def __init__(self, name):
		self.name = name
		self.keywords = []
		self.scores = []
		self.score = 0.0

	def score_incantation(self, incantation):
		self.scores = [0.0] * len(self.keywords)
		self.score = 0.0

		for i in range(len(self.keywords)):
			if use_dummy_model:
				self.scores[i] = random.random() * 3.0
			else:
				keyword = self.keywords[i]
				self.scores[i] = web_model.predict([(incantation, keyword)])[0]

		# self.score = self.avg(self.scores)
		self.score = self.top(self.scores)

	def avg(self, list):
		return sum(list) / len(list)

	def top2_avg(self, list):
		list.sort()
		list.reverse()
		return self.avg(list[0:2])

	def top(self, list):
		list.sort()
		list.reverse()
		return list[0]

class SpellMan:
	def __init__(self):
		self.sm_light = SpellMod("light")
		self.sm_light.keywords = ["fire", "heat", "strength"]
		self.sm_extinguish = SpellMod("extinguish")
		self.sm_extinguish.keywords = ["smother", "water", "reduce"]

def check(incantation, spell_mod):
	spell_mod.score_incantation(incantation)
	print("  %s: %f" % (spell_mod.name, spell_mod.score))
	for i in range(len(spell_mod.keywords)):
		print("     %s: %f" % (spell_mod.keywords[i], spell_mod.scores[i]))

def run_cli():
	spell_man = SpellMan()

	while True:
		incantation = input('Speak: ')
		if incantation == "q":
			break
		check(incantation, spell_man.sm_light)
		check(incantation, spell_man.sm_extinguish)
		print()


def signal_handler(signal, frame):
	global interrupted
	interrupted = True

def run_unity_client():
	print("Running unity client")

	signal.signal(signal.SIGINT, signal_handler)

	context = zmq.Context()
	socket = context.socket(zmq.REP)
	socket.bind("tcp://*:5555")

	poller = zmq.Poller()
	poller.register(socket, zmq.POLLIN)

	spell_man = SpellMan()

	try:
		while True:
			socks = dict(poller.poll(timeout=100))  # Timeout in milliseconds

			if socket in socks and socks[socket] == zmq.POLLIN:
				message = socket.recv()

				if message != "":
					print("Received request: %s" % message)
						
					scores = []
					check(str(message), spell_man.sm_light)
					scores.append(spell_man.sm_light.score)
					check(str(message), spell_man.sm_extinguish)
					scores.append(spell_man.sm_extinguish.score)

					socket.send_string(str(scores))

	except KeyboardInterrupt:
		pass

if use_cli:
	run_cli()
else:
	run_unity_client()
