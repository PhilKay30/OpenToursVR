from flask import Blueprint
from flask import Flask, abort, request, jsonify
import json
from DbUtils import DbUtils

db_api = Blueprint("db_api", __name__)

# Function: addimg
# Description: API routed with a POST method.
#              This route inserts the image into the database.
# Parameters: JSON POST: The information required to insert the object. 
# Return: A generic result message if it does not have a 500 series error.
@db_api.route("/addimg", methods=["POST"])
def addimg():
    if not request.json:
        abort(400)

    imgData = json.dumps(request.json)
    imgObject = json.loads(imgData, object_hook=JSONObject)
    dbUtils = DbUtils()

    dbUtils.addNewPNG(
        imgObject.img_name,
        list(imgObject.img),
        imgObject.img_size,
        imgObject.corner,
        imgObject.img_rotation,
    )

    return jsonify({"Result": "Inserted"}), 201

# Function: getimg
# Description: API routed with a GET method.
#              This route gets the image the user requested
# Parameters: img_name: the name of the image requested
# Return: a json object containing the img_name, img, img_size, bot_left_corner and its rotation
@db_api.route("/addhistorical", methods=["POST"])
def addhistorical():
    if not request.json:
        abort(400)

    distanceData = json.dumps(request.json)
    distanceObject = json.loads(distanceData, object_hook=JSONObject)
    dbUtils = DbUtils()

    dbUtils.addDistance(
        distanceObject.length,
        distanceObject.width
    )

# Function: getimg
# Description: API routed with a GET method.
#              This route gets the image the user requested
# Parameters: img_name: the name of the image requested
# Return: a json object containing the img_name, img, img_size, bot_left_corner and its rotation
@db_api.route("/getimg/<string:img_name>", methods=["GET"])
def getimg(img_name):
    from DbUtils import DbUtils

    img = []
    dbUtils = DbUtils()
    imgData = dbUtils.getPNG(img_name)
    # Parse the info and return it
    for r in imgData:
        a = {
            "img_name": r[0],
            "img": r[1],
            "img_size": r[2],
            "bot_left_corner": r[3],
            "rotation": r[4],
        }
        img.append(a)
    return jsonify(img)


# Function: getbounds
# Description: API routed with a GET method.
#              This route gets the longitude and latitude of the 4 sides of the map
# Parameters: None
# Return: a json object with the 4 bounds of the osm map
@db_api.route("/getbounds", methods=["GET"])
def getbounds():
    # Init my variables that i am going to be using
    dbUtils = DbUtils()
    top = ""
    right = ""
    bottom = ""
    left = ""
    edges = []

    # Get my bounds from the server
    boundData = dbUtils.getBounds()

    # parse my boundData and set each row to a longitute and latitude in a list of dictionaries.
    for r in boundData:
        # get longitude out
        tempStrings = str(r).split()
        tempStrings[0] = tempStrings[0][8:]
        # get latitude out
        index = tempStrings[1].find(")")
        tempStrings[1] = tempStrings[1][0:index]
        # add that to a dictionary
        a = {"lon": tempStrings[0], "lat": tempStrings[1]}
        # add the dictionary to a list
        edges.append(a)

    # Check the latitudes for a top and bottom
    if float(edges[0]["lat"]) > float(edges[1]["lat"]):
        top = edges[0]["lat"]
        bottom = edges[1]["lat"]
    else:
        top = edges[1]["lat"]
        bottom = edges[0]["lat"]

    # Check longitude for a left and right
    if float(edges[0]["lon"]) < float(edges[1]["lon"]):
        left = edges[0]["lon"]
        right = edges[1]["lon"]
    else:
        right = edges[0]["lon"]
        left = edges[1]["lon"]

    # Create my json dictionary
    bounds = {"top": top, "bottom": bottom, "left": left, "right": right}

    # return the json to the user
    return jsonify(bounds)

# Name: JSONObject
# Description: This object is a hook into a json object to make a dynamic class size object with fields that are the JSON key's
#              and their values are the values of the keys
# Parameters: Self: it takes itself. Dict: the JSON dictionary.
class JSONObject:
    def __init__(self, dict):
        vars(self).update(dict)
