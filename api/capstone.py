from flask import Flask
from DbService import db_api

app = Flask(__name__)
app.register_blueprint(db_api, url_prefix="/db_api")


@app.route("/")
def service():
    return "Service is running!"


if __name__ == "__main__":
    app.run("0.0.0.0")
