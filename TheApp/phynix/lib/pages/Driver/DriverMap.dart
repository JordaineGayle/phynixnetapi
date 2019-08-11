
import 'dart:async';
import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:location/location.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import 'package:phynix/Connectors/Auth.dart';
import 'package:phynix/Connectors/Driver.dart';
import 'package:phynix/Controllers/SignalR.dart';
import 'package:shared_preferences/shared_preferences.dart';

class DriverMap extends StatefulWidget {
  State<DriverMap> createState() => DriverMapState();
}

class DriverMapState extends State<DriverMap> {

  Completer<GoogleMapController> _controller = Completer();
  String dropdownValue = 'Offline';

  static final CameraPosition kingston = CameraPosition(
    target: LatLng(18.0235484, -76.742409),
    zoom: 11.71,
  );

  DriverMapState() {
    updateLocation();

    initializeSignalR();
  }

  initializeSignalR() async {
    Map<String, dynamic> data = await Auth.signalRAuth();

    SignalR(data['url'], data['accessToken'], context);
    print(data);

  }


  updateLocation() {

    var location = new Location();

    location.onLocationChanged().listen((LocationData currentLocation) {
      print(currentLocation.latitude);
      print(currentLocation.longitude);

      final CameraPosition update = CameraPosition(
        target: LatLng(currentLocation.latitude, currentLocation.longitude),
        zoom: 14.71,
      );

      print(currentLocation.longitude);
      updateLocationData(update);

    });


  }

  updateLocationData(CameraPosition update) async {

    SharedPreferences pref = await SharedPreferences.getInstance();
    String data = pref.getString('user');
    Map<String, dynamic> userData = json.decode(data);

    Map<String, dynamic> map = Map();

    map['DriverId'] = userData['id'];
    map['Latitude'] = update.target.latitude;
    map['Longitude'] = update.target.longitude;
    map['Status'] = userData['status'];


    Driver.broadcastLocation(map);

              final GoogleMapController controller = await _controller.future;
              controller.animateCamera(CameraUpdate.newCameraPosition(update));
  }

  @override
  Widget build(BuildContext context) {
    // TODO: implement build
    return Scaffold(
      appBar: AppBar(),
      drawer: Drawer(
        child: ListView(
          children: <Widget>[
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
            ),
            ListTile(
              title: Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: <Widget>[
                  Text("Status"),
                  DropdownButton<String>(
                    value: dropdownValue ,
                    onChanged: (String newValue) async {
                      setState(() {
                        dropdownValue = newValue;
                      });

                      SharedPreferences pref = await SharedPreferences.getInstance();

                      String userData = pref.getString('user');
                      Map<String, dynamic> map = json.decode(userData);

                      map['status'] = newValue;
                      pref.setString('user', json.encode(map));

                      },
                    items: <String>['Offline', 'Online', 'Break'].map<DropdownMenuItem<String>>(
                            (String value){
                              return DropdownMenuItem<String>(
                                value: value,
                                child: Text(value),
                              );
                            }
                    ).toList(),
                  )
                ],
              ),
            )
          ],
        ),
      ),
      body: GoogleMap(
        mapType: MapType.normal,
        myLocationEnabled: true,
        myLocationButtonEnabled: true,
        initialCameraPosition: kingston,
        onMapCreated: (GoogleMapController controller) {
          _controller.complete(controller);
        },
      ),
    );
  }

}