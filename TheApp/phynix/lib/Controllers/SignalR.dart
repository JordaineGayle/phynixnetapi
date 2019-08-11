// Import the library.
import 'dart:async';
import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:geocoder/geocoder.dart';
import 'package:location/location.dart';
import 'package:phynix/Connectors/Driver.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:signalr_client/signalr_client.dart';

class SignalR {

  String _serverUrl;
  String _accessToken;
  HubConnection _hubConnection;
  BuildContext _context;

  SignalR(this._serverUrl, this._accessToken, this._context) {
    openConnection();
  }

  openConnection() {
    print("sig $_accessToken");
    if(_hubConnection == null) {

      _hubConnection = HubConnectionBuilder().withUrl(_serverUrl, options: HttpConnectionOptions(accessTokenFactory: () async => _accessToken)).build();

      _hubConnection.onclose((error) => print("hello"));
      
      _hubConnection.on('newrider', _handleIncommingChatMessage);
      _hubConnection.on('cancel', _cancelMethod);
    }

    if(_hubConnection.state != HubConnectionState.Connected){
      _hubConnection.start();
      print("Signal opened");
    }
  }

  _cancelMethod(List<Object> args) {
    print(args);
    print("21");

    Navigator.pop(_context);
  }
  _handleIncommingChatMessage(List<Object> args){
    print(args);

    args.forEach((value) async {

      Map<String, dynamic> data = value;

      // From coordinates
      final coordinates = new Coordinates(data['Lat'], data['Lng']);
      var addresses = await Geocoder.local.findAddressesFromCoordinates(coordinates);
      var first = addresses.first.addressLine;

      showDialog(
          context: _context,
          builder: (_context){
            return AlertDialog(
              title: Text("New Trip"),
              content: Text("${data['FirstName']} would like a taxi for hire to $first"),
              actions: <Widget>[
                RaisedButton(
                  onPressed: () async {

                    LocationData currentLocation = await Location().getLocation();
                    SharedPreferences pref = await SharedPreferences.getInstance();
                    var userData = json.decode(pref.getString('user'));


                    Map<String, dynamic> mapData = {};

                    mapData['Origin'] = {"Latitude": data['Lat'], "Longitude": data['Lng']};
                    mapData['Destination'] = {"Latitude" : currentLocation.latitude, "Longitude": currentLocation.longitude};
                    mapData['UserId'] = data['Id'];
                    mapData['DriverId'] = userData['id'];
                    mapData['Total'] = 3234.0;

                    Driver.acceptRider(mapData);
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
            );
          });

    });


  }


}