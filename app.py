from flask import Flask, request, jsonify
from transformers import DistilBertTokenizer, DistilBertForSequenceClassification
import torch

app = Flask(__name__)

tokenizer = DistilBertTokenizer.from_pretrained("fine_tuned_distilbert")
model = DistilBertForSequenceClassification.from_pretrained("fine_tuned_distilbert")

@app.route("/classify", methods=["POST"])
def classify_query():
    text = request.json.get("text")
    inputs = tokenizer(text, return_tensors="pt", truncation=True, padding=True)
    with torch.no_grad():
        outputs = model(**inputs)
    logits = outputs.logits
    predicted_class = torch.argmax(logits, dim=-1).item()
    return str(predicted_class), 200

if __name__ == "__main__":
    app.run(debug=True)
