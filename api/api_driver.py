from flask import Flask, jsonify, request, abort
from flask_sqlalchemy import SQLAlchemy
from flask_migrate import Migrate
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
    __tablename__ = "map_bounds"

    map_name = db.Column(db.String(), primary_key=True)
    top_left = db.Column(db.String()) # Will represent a 'POINT()'
    bottom_right = db.Column(db.String()) # Will represent a 'POINT()'

    def __init__(
        self,
        map_name,
        top_left,
        bottom_right,
    ):
        self.map_name = map_name,
        self.top_left = top_left,
        self.bottom_right = bottom_right,

    
    def __repr__():
        return f"Top Left : {self.top_left}  Bottom Right : {self.bottom_right}"


#
#
class Points(db.Model):
    __tablename__ = "data_points"
    
    point_id = db.Column(db.Integer(), primary_key=True)
    point_location = db.Column(db.String(), nullable=False) # Will represent a 'POINT()'
    point_name = db.Column(db.String(), nullable=False)
    point_desc = db.Column(db.String(), nullable=False)
    point_image = db.Column(db.String(), nullable=False)

    def __init__(
        self, 
        point_location,
        point_name,
        point_desc,
        point_image
    ):
        self.point_location = point_location
        self.point_name = point_name,
        self.point_desc = point_desc,
        self.point_image = point_image
    
    
    def __repr__():
        return f"Name: {self.point_name} ID:{self.point_id} Point: {self.point_location}"


# Name: Images
# Description: This class represents the images table that is in the database 
class Images(db.Model):
    __tablename__ = "map_images"

    image_name = db.Column(db.String(), primary_key=True)
    image_data = db.Column(db.String())
    image_size = db.Column(db.Integer())
    bottom_left_corner = db.Column(db.String()) # String as geoalchemy2 doesnt like sqlalchemy and migrate
    image_rotation = db.Column(db.Float())
    km_width = db.Column(db.Float())
    km_height = db.Column(db.Float())

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
        ) 
        .filter(Images.image_name == image_name)
        .all() 
    )

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
    if not request.json:
        abort(400)
    
    boundData = json.dumps(request.json)
    boundObject = json.loads(boundData, object_hook=JSONObject)

    map_bounds = Bounds(
        boundObject.map_name,
        boundObject.top_left,
        boundObject.bottom_right,
    )

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
        if insert:
            ret = "Inserted"
        elif update:
            ret = "Updated"
        else:
            ret = "Error, could not be inserted or updated"

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

    # parse the top left and bottom right into 4 specific sides
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


# Add Point Route
# Method: POST
# Description: Recieves JSON request, parses it, and adds the point to the database.
@app.route("/addpoint/", methods=["POST"])
def add_point():
    if not request.json:
        abort(400)
    
    point_data = json.dumps(request.json)
    point_object = json.loads(point_data, object_hook=JSONObject)

    point = Points(
        point_object.point_location,
        point_object.point_name,
        point_object.point_desc,
        point_object.point_image,        
    )

    insert = False
    update = False
    ret = ""
    
    try:
        db.session.add(point)
        db.session.commit()
        insert = True
        print("Inserted")
    except:
        db.session.rollback()
        print("Inserting failed, rolling back transaction")
    finally:
        #Build the return
        if insert:
            ret = "Inserted"
        else:
            ret = "Error, could not be inserted or updated"

    return {"Status": ret}    


# Get All Points Route
# Method: GET
# Description: This returns the ID and Point of all the points.
@app.route("/getpoint/", methods=["GET"])
def get_points():
    query = (
        Points.query.with_entities(
            Points.point_id,
            Points.point_location,
        ).all()
    )

    results = [
        {
            "point_id": q.point_id,
            "point_location": q.point_location,
        }
        for q in query
    ]

    return {"Result": results}


# Get Data From Points
# Methods: GET
# Description: This returns the Name, Description and Image of the point
@app.route("/getpoint/<string:point_id>", methods=["GET"])
def get_point(point_id):
    query = (
        Points.query.with_entities(
            Points.point_name,
            Points.point_desc,
            Points.point_image,
        ).filter(Points.point_id == point_id).all()
    )

    results = [
        {
            "point_name": q.point_name,
            "point_desc": q.point_desc,
            "point_image": q.point_image,
        }
        for q in query
    ]

    return {"Result": results}


# If this is the main file being called, run the app.
if __name__ == "__main__":
    app.run("0.0.0.0")
