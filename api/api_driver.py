from flask import Flask, jsonify
from flask_sqlalchemy import SQLAlchemy
from flask_migrate import Migrate
import json 


connect_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/osm_map"

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = connect_string
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False
db = SQLAlchemy(app)
migrate = Migrate(app, db)


class Images(db.Model):
    __tablename__ = "map_images"

    image_name = db.Column(db.String(), primary_key=True)
    image_data = db.Column(db.String())
    image_size = db.Column(db.Integer())
    bottom_left_corner = db.Column(db.String())
    image_rotation = db.Column(db.Float())

    def __init__(self, image_name, image_data, image_size, bottom_left_corner, image_rotation=0.0):
        self.image_name = image_name
        self.image_data = image_data
        self.image_size = image_size
        self.bottom_left_corner = bottom_left_corner
        self.image_rotation = image_rotation

    def __repr__(self):
        return f"image_name :{self.image_name} {self.image_size}"





@app.route('/')
def service_route():
    return "<h1>Service Running</h1>"


@app.route("/getimg/<string:image_name>")
def get_image(image_name):
    query = Images.query.all()
    results = [
            {
                "image_name": q.image_name,
                "image_size": q.image_size,
                "image_data": q.image_data,
                "image_rotation": q.image_rotation,
                "bottom_left_corner": q.bottom_left_corner
            } for q in query]
    
    return {"Result":results}


if __name__ == '__main__':
    app.run()



