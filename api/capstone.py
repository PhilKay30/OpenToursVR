from flask import Flask
from DbService import db_api
from flask_sqlalchemy import SQLAlchemy

# from flask_migrate import Migrate


app = Flask(__name__)


# remove api url prefix, not needed
app.register_blueprint(db_api)  # url_prefix="/db_api"


# db = SQLAlchemy(app)
# migrate = Migrate(app, db)


@app.route("/")
def service():
    return "Service is running!"


if __name__ == "__main__":
    app.run("0.0.0.0")
