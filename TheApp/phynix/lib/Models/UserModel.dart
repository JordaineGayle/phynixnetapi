
import 'dart:core';

class UserModel {
  String id;
  String FirstName;
  String LastName;
  String Email;
  String Phone;
  String Status;
  bool IsActivated;
  bool IsLoggedIn;
  DateTime DateCreated;
  DateTime LastModified;

  UserModel(this.FirstName, this.LastName, this.Email);

}