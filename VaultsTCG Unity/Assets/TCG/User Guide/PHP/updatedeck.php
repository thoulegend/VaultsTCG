<?php

$userid = $_REQUEST['userid'];
$deckstring = $_REQUEST['deckstring'];  

$host = 'localhost';
$user = 'secondhand_tcg';
$dbpassword = '111111';
$db = 'secondhand_tcg'; 
$table = 'users'; 

mysql_connect($host, $user, $dbpassword) or die(mysql_error()); 
mysql_select_db($db);


$deletedeck = mysql_query("DELETE FROM `player_decks` WHERE `user_id` ='".$userid."'" ) or die (mysql_error()); //deleting old deck

$cards =  explode(',', $deckstring);


foreach ($cards as $card)
   {
  if (strlen($card)>0) { mysql_query("INSERT INTO `player_decks` VALUES ('NULL', '{$userid}', '{$card}')") or die (mysql_error()); } //adding cards to the deck deck one by one
   }


echo "DECK-SUCCESS";



?>
