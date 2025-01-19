from flask import Flask, request, jsonify
from transformers import BertTokenizer, BertForSequenceClassification
import torch

app = Flask(__name__)

# Load model and tokenizer
tokenizer = BertTokenizer.from_pretrained("fine_tuned_bert")
model = BertForSequenceClassification.from_pretrained("fine_tuned_bert")

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
