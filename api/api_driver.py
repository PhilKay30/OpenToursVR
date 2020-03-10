# Import Flask, jsonify, request from flask
from flask import Flask, jsonify, request, abort
# import sqlalchemy from flask_sqlalchemy 
from flask_sqlalchemy import SQLAlchemy
# Import migrate from flask_migrate
from flask_migrate import Migrate
# Import json to use that
import json 

# Connection String to the PostgreSQL Database
connect_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/osm_map"

# Set up the app, configure it with the connection string and disable tracking modifications.
app = Flask(__name__)
app.config["SQLALCHEMY_DATABASE_URI"] = connect_string
app.config["SQLALCHEMY_TRACK_MODIFICATIONS"] = False
# create the SQLAlchemy database.
db = SQLAlchemy(app)
# create the Migrate with the app and the DB
migrate = Migrate(app, db)


# Name: JSONObject
# Description: This is a hook object that will self build based on the JSON that came in.
class JSONObject:
    # Constructor
    def __init__(self, dict):
        vars(self).update(dict)


# Name: Bounds
# Description: This class represents the bounds table.
class Bounds(db.Model):
    # Table Name
    __tablename__ = "map_bounds"

    # Columns
    map_name = db.Column(db.String(), primary_key=True)
    top_left = db.Column(db.String()) # Will represent a 'POINT()'
    bottom_right = db.Column(db.String()) # Will represent a 'POINT()'

    # Constructor
    def __init__(
        self,
        map_name,
        top_left,
        bottom_right,
    ):
        self.map_name = map_name,
        self.top_left = top_left,
        self.bottom_right = bottom_right,

# Name: Images
# Description: This class represents the images table that is in the database 
class Images(db.Model):
    # Table Name
    __tablename__ = "map_images"

    # Columns
    image_name = db.Column(db.String(), primary_key=True) #Primary Key
    image_data = db.Column(db.String())
    image_size = db.Column(db.Integer())
    bottom_left_corner = db.Column(db.String()) # String as geoalchemy2 doesnt like sqlalchemy and migrate
    image_rotation = db.Column(db.Float())
    km_width = db.Column(db.Float())
    km_height = db.Column(db.Float())

    # Constructor with default image_rotation set to 0.0f
    def __init__(
        self,
        image_name,
        image_data,
        image_size,
        bottom_left_corner,
        km_height,
        km_width,
        image_rotation=0.0,
    ):
        self.image_name = image_name
        self.image_data = image_data
        self.image_size = image_size
        self.bottom_left_corner = bottom_left_corner
        self.image_rotation = image_rotation
        self.km_height = km_height
        self.km_width = km_width
    
    # Debug print
    def __repr__(self):
        return f"image_name :{self.image_name} {self.image_size}"


# The base route of the app
@app.route("/")
def service_route():
    return "<h1>Service Running</h1>"


# GetImage Route, using a image_name as the paramter.
@app.route("/getimg/<string:image_name>")
def get_image(image_name):
    # Create the query
    query = (
        Images.query.with_entities(
            Images.image_name,
            Images.image_data,
            Images.image_size,
            Images.image_rotation,
            Images.km_height,
            Images.km_width,
            Images.bottom_left_corner,
        ) # Add the filter
        .filter(Images.image_name == image_name)
        .all() 
    )

    # Build the results
    results = [
        {
            "image_name": q.image_name,
            "image_size": q.image_size,
            "image_data": q.image_data,
            "image_rotation": q.image_rotation,
            "bottom_left_corner": q.bottom_left_corner,
            "km_width": q.km_width,
            "km_height": q.km_height,
        }
        for q in query
    ]

    #return
    return {"Result": results}


@app.route("/addimg/", methods=["POST"])
def add_img():
    #Sanity Check
    if not request.json:
        abort(400)

    # Dump the json into a string
    imgData = json.dumps(request.json)
    # Build a json object from a string
    imgObject = json.loads(imgData, object_hook=JSONObject)
    # Build the Image
    map_image = Images(
        imgObject.image_name,
        imgObject.image_data,
        imgObject.image_size,
        imgObject.bottom_left_corner,
        imgObject.km_height,
        imgObject.km_width,
        imgObject.image_rotation,
    )
    print("Images made")
    insert = False
    update = False
    ret = ""

    # Insert, if that throws an exception, update instead.
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
                Images.image_name == map_image.image_name
            ).update(
                {
                    "image_data": imgObject.image_data,
                    "image_size": imgObject.image_size,
                    "bottom_left_corner": imgObject.bottom_left_corner,
                    "km_height": imgObject.km_height,
                    "km_width": imgObject.km_width,
                    "image_rotation": imgObject.image_rotation,
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
        #Build the return
        if insert:
            ret = "Inserted"
        elif update:
            ret = "Updated"
        else:
            ret = "Error, could not be inserted or updated"

    # return
    return {"Status": ret}


# Route for the insert bounds
@app.route("/addbounds/", methods=["POST"])
def add_bounds():
    # Sanity check
    if not request.json:
        abort(400)
    
    boundData = json.dumps(request.json)
    boundObject = json.loads(boundData, object_hook=JSONObject)

    map_bounds = Bounds(
        boundObject.map_name,
        boundObject.top_left,
        boundObject.bottom_right,
    )

    # Log object built
    insert = False
    update = False
    ret = ""

    try:
        db.session.add(map_bounds)
        db.session.commit()
        insert = True
        print("Inserted")
    except:
        db.session.rollback()
        print("Roll")
        try:
            print("Trying To Update")
            db.session.query(Bounds).filter(
                Bounds.map_name == map_bounds.map_name
            ).update(
                {
                    "top_left": map_bounds.top_left,
                    "bottom_right": map_bounds.bottom_right,
                }
            )
            db.session.commit()
            update = True
            print(f"updated: {update}")
        except Exception as e:
            print(e)
    finally:
        #Build the return
        if insert:
            ret = "Inserted"
        elif update:
            ret = "Updated"
        else:
            ret = "Error, could not be inserted or updated"

    # Return
    return {"Status": ret}


# GetImage Route, using a image_name as the paramter.
@app.route("/getbounds/<string:map_name>")
def get_bounds(map_name):
    # Create the query
    query = (
        Bounds.query.with_entities(
            Bounds.map_name,
            Bounds.top_left,
            Bounds.bottom_right,
        ) # Add the filter
        .filter(Bounds.map_name == map_name)
        .all() 
    )

    # Build the results
    results = [
        {
            "map_name": q.map_name,
            "top_left": q.top_left,
            "bottom_right": q.bottom_right,
        }
        for q in query
    ]

    #return
    return {"Result": results}


# If this is the main file being called, run the app.
if __name__ == "__main__":
    app.run("0.0.0.0")
