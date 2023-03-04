import random

use_dummy_model = 0

if not use_dummy_model:
    from ..external.semantic_text_similarity.semantic_text_similarity.models import WebBertSimilarity
    web_model = WebBertSimilarity(device='cpu', batch_size=10) #defaults to GPU prediction

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
                self.scores[i] = web_model.predict([(incantation, self.keywords[i])])

        # self.score = self.avg(self.scores)
        self.score = self.top2_avg(self.scores)

    def avg(self, list):
        return sum(list) / len(list)

    def top2_avg(self, list):
        list.sort()
        list.reverse()
        return self.avg(list[0:2])


def check(incantation, spell_mod):
    spell_mod.score_incantation(incantation)
    print("  %s: %f" % (spell_mod.name, spell_mod.score))
    for i in range(len(spell_mod.keywords)):
        print("     %s: %f" % (spell_mod.keywords[i], spell_mod.scores[i]))


sm_light = SpellMod("light")
sm_light.keywords = ["illuminate", "endure", "holy"]
sm_extinguish = SpellMod("extinguish")
sm_extinguish.keywords = ["void", "dark", "extinguish"]

while True:
    incantation = input('Speak: ')
    if incantation == "q":
        break
    check(incantation, sm_light)
    check(incantation, sm_extinguish)
    
