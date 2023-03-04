from ..external.semantic_text_similarity.semantic_text_similarity.models import WebBertSimilarity
# from ..external.semantic_text_similarity.semantic_text_similarity.models import ClinicalBertSimilarity

web_model = WebBertSimilarity(device='cpu', batch_size=10) #defaults to GPU prediction

print('hurray!')

def check(x, y):
   r = web_model.predict([(x,y)])
   print("%s, %s: %f" % (x, y, r))

x = "illuminate"
check(x, "glow")
