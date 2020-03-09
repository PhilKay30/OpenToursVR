from flask import Flask, jsonify, request
from flask_sqlalchemy import SQLAlchemy
from flask_migrate import Migrate
import json 


connect_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/osm_map"

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = connect_string
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False
db = SQLAlchemy(app)
migrate = Migrate(app, db)


class JSONObject:
    def __init__(self, dict):
        vars(self).update(dict)


class Images(db.Model):
    __tablename__ = "map_images"

    image_name = db.Column(db.String(), primary_key=True)
    image_data = db.Column(db.String())
    image_size = db.Column(db.Integer())
    bottom_left_corner = db.Column(db.String())
    image_rotation = db.Column(db.Float())
    km_width = db.Column(db.Float())
    km_height = db.Column(db.Float())  

    def __init__(self, image_name, image_data, image_size, bottom_left_corner, km_height, km_width, image_rotation=0.0):
        self.image_name = image_name
        self.image_data = image_data
        self.image_size = image_size
        self.bottom_left_corner = bottom_left_corner
        self.image_rotation = image_rotation
        self.km_height = km_height
        self.km_width = km_width

    def __repr__(self):
        return f"image_name :{self.image_name} {self.image_size}"


@app.route('/')
def service_route():
    return "<h1>Service Running</h1>"


@app.route("/getimg/<string:image_name>")
def get_image(image_name):
    query = Images.query.with_entities(
        Images.image_name,
        Images.image_data,
        Images.image_size,
        Images.image_rotation,
        Images.km_height,
        Images.km_width,
        Images.bottom_left_corner
    ).filter(Images.image_name == image_name).all()

    results = [
            {
                "image_name": q.image_name,
                "image_size": q.image_size,
                "image_data": q.image_data,
                "image_rotation": q.image_rotation,
                "bottom_left_corner": q.bottom_left_corner,
                "km_width": q.km_width,
                "km_height": q.km_height
            } for q in query]
    
    return {"Result":results}



@app.route("/addimg/", methods=["POST"])
def add_img():
    if not request.json:
        abort(400)
    #print(request.json)
    imgData = json.dumps(request.json)
    imgObject = json.loads(imgData, object_hook=JSONObject)
    map_image = Images(
        imgObject.image_name,
        imgObject.image_data,
        imgObject.image_size,
        imgObject.bottom_left_corner,
        imgObject.km_height,
        imgObject.km_width,
        imgObject.image_rotation
    )
    print("Images made")
    insert = False
    update = False
    ret= ""

    try:
        print("AddingImage")
        db.session.add(map_image)
        db.session.commit()
        insert = True
        print("Inserted")
    except:
        db.session.rollback()
        print("Roll")
        try:
            print("Trying To Update")
            db.session.query(Images).filter(
                Images.image_name == map_image.image_name).update(
                    {
                        "image_data":imgObject.image_data,
                        "image_size":imgObject.image_size,
                        "bottom_left_corner":imgObject.bottom_left_corner,
                        "km_height":imgObject.km_height,
                        "km_width":imgObject.km_width,
                        "image_rotation":imgObject.image_rotation
                    }
                )
            print("")
            db.session.commit()
            update = True
            print(f"updated: {update}")
        except Exception as e:
            print(e)
            db.session.rollback()
    finally:
        if insert:
            ret = "Inserted"
        elif update:
            ret = "Updated"
        else:
            ret = "Error, could not be inserted or updated"

    return {"Status": ret}

if __name__ == '__main__':
    app.run("0.0.0.0")


