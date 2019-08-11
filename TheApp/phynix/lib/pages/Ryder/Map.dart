
import 'dart:async';
import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:geocoder/geocoder.dart';
import 'package:geolocator/geolocator.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import 'package:google_places_picker/google_places_picker.dart';
import 'package:location/location.dart';
import 'package:phynix/Connectors/Auth.dart';
import 'package:phynix/Connectors/Rider.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter_polyline_points/flutter_polyline_points.dart';


class RMap extends StatefulWidget {
  State<RMap> createState() => MapState();
}

class MapState extends State<RMap> {
  Completer<GoogleMapController> _controller = Completer();
  Map<MarkerId, Marker> markers = Map<MarkerId, Marker>();
  final Set<Polyline> _polyline = {};
  List<LatLng> latlng = List();
  LatLng destination;
  LatLng origin;

  MapState(){
    PluginGooglePlacePicker.initialize(
      androidApiKey: "AIzaSyDruzCHBfmtZuEuQZtvxGiXODYGkI3xSUo",
      iosApiKey: "AIzaSyDruzCHBfmtZuEuQZtvxGiXODYGkI3xSUo",
    );


  }

  static final CameraPosition kingston = CameraPosition(
    target: LatLng(18.0235484, -76.742409),
    zoom: 11.71,
  );

  PoliData(LatLng latLng) async {
    LocationData currentLocation = await Location().getLocation();
    PolylinePoints polylinePoints = PolylinePoints();
    List<PointLatLng> result = await polylinePoints.getRouteBetweenCoordinates("AIzaSyDruzCHBfmtZuEuQZtvxGiXODYGkI3xSUo",
        currentLocation.latitude, currentLocation.longitude, latLng.latitude, latLng.longitude);

    print(result);
    List<LatLng> _latlng = List();

    result.forEach((PointLatLng value){
      _latlng.add(LatLng(value.latitude, value.longitude));
    });

    setState(() {
      latlng = _latlng;
      _polyline.add(Polyline(polylineId: PolylineId("hi"),
          visible: true,
          width: 4,
          points: latlng,
          color: Colors.blue
      ));
    });

    String origin = await _addressFromCordinates(currentLocation.latitude, currentLocation.longitude);
    String desti = await _addressFromCordinates(destination.latitude, destination.longitude);
    double distanceInMeters = await Geolocator().distanceBetween(currentLocation.latitude, currentLocation.longitude, destination.latitude, destination.longitude);
    double distanceInKM = distanceInMeters / 1000;
    double fare = 80 * distanceInKM;
    // alert box
    _showDialog(origin, desti, distanceInKM, fare);

  }

  _addressFromCordinates(double lat, double lng) async {
    // From coordinates
    final coordinates = new Coordinates(lat, lng);
    var addresses = await Geocoder.local.findAddressesFromCoordinates(coordinates);
    var first = addresses.first.addressLine;
    return first;
  }

  _showDialog(String origin, String destination, double miles, double fare) {
    showDialog(
        context: context,
        builder: (_context){
          return Container(
              height: 200,
              margin: EdgeInsets.symmetric(vertical: 50),
              child: AlertDialog(
            title: Text("New Trip"),
            content: Text("From:$origin To: $destination \n Total Miles: ${miles.toStringAsFixed(2)} \n Fare:\$ ${fare.toStringAsFixed(2)}"),
            actions: <Widget>[
              RaisedButton(
                onPressed: () async {

                },
                child: Text("Confirm", style: TextStyle(color: Colors.white),),
              ),
              RaisedButton(
                onPressed: () {
                  Navigator.pop(_context);
                },
                child: Text("Cancel", style: TextStyle(color: Colors.white),),
              )
            ],
          ));
        });
  }


  @override
  Widget build(BuildContext context) {
    // TODO: implement build
    return Scaffold(
      appBar: AppBar(),
      drawer: Drawer(
        child: ListView(
          children: <Widget>[
            DrawerHeader(
              child: Text("Hello John"),
            ),
            ListTile(
              onTap: () async {
                SharedPreferences pref = await SharedPreferences.getInstance();
                pref.remove('token');
                Navigator.pushNamedAndRemoveUntil(context, 'login', (Route<dynamic> route) => false);
              },
              title: Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: <Widget>[
                  Icon(Icons.power),
                  Text("Logout")
                ],
              ),
            )
          ],
        ),
      ),
      body: Stack(
        children: <Widget>[

          GoogleMap(
            mapType: MapType.normal,
            polylines: _polyline,
            myLocationEnabled: true,
            myLocationButtonEnabled: true,
            markers: Set<Marker>.of(markers.values),
            initialCameraPosition: kingston,
            onMapCreated: (GoogleMapController controller) {
              _controller.complete(controller);
            },
          ),
          Align(
            alignment: Alignment.bottomCenter,
              child:
                  Container(
                    margin: EdgeInsets.all(80),
                    child: RaisedButton(onPressed: () async {



                      Place place = await PluginGooglePlacePicker.showAutocomplete(
                        mode: PlaceAutocompleteMode.MODE_OVERLAY,

                      );

                      final Marker _marker = Marker(markerId: MarkerId(place.id)
                          ,position: LatLng(place.latitude, place.longitude),
                          infoWindow: InfoWindow(title: place.name,)
                          ,onTap: () {

                          }
                      );

                      var location = new Location();

                      location.onLocationChanged().listen((LocationData currentLocation) {
                        setState(() {
                          destination = LatLng(place.latitude, place.longitude);
                          origin = LatLng(currentLocation.latitude, currentLocation.longitude);


                        });

                      });

                        PoliData(LatLng(place.latitude, place.longitude));





                      setState(() {
                        markers[MarkerId(place.id)] = _marker;
                      });

                      Map<String, dynamic> map = Map<String, dynamic>();
                      SharedPreferences pref = await SharedPreferences.getInstance();
                      Map<String, dynamic> userData = json.decode(pref.getString('user'));

                      map['FirstName'] = userData['firstname'];
                      map['LastName'] = userData['lastname'];
                      map['Phone'] = userData['phone'];
                      map['Id'] = userData['id'];
                      map['Lat'] = place.latitude;
                      map['Lng'] = place.longitude;

                      Ryder.submitLocation(map);

                      print("lat ${place.address}");
                    },
                      child: Text("Pick Me Up"),),
                  ),
              )
        ],
      )
    );
  }

}