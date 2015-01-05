<?php 
$userid = $_REQUEST['userid']; 
$index = $_REQUEST['index']; 

$host = 'localhost'; 
$user = 'secondhand_tcg';  
$dbpassword = '111111';
$db = 'secondhand_tcg'; 

mysql_connect($host, $user, $dbpassword) or die(mysql_error());
mysql_select_db($db); 
 $hash = $_GET['hash']; 
 
        $secretKey="WorkSucks"; # Change this value to match the value stored in the client javascript below 

        $real_hash = md5($userid . $silver . $secretKey); 
        //if($real_hash == $hash) { 

		
		$findcost = mysql_query("SELECT * FROM `promo_cards` WHERE `card_id`='".$index."'" ) or die (mysql_error());
$numrows = mysql_num_rows($findcost);

if ($numrows == 0)
{

		die (" the card not found \n");
}
else
{


while($row = mysql_fetch_assoc($findcost))
  {
 $cost = $row['cost'] ;  break;
  }
  
  $check = mysql_query("SELECT * FROM `silver` WHERE `user_id`='".$userid."'" ) or die (mysql_error());
$numrows = mysql_num_rows($check);
if ($numrows == 0)
{
		die ("User id ".$userid." doesn't have silver \n");
}

else 
{
while ($row = mysql_fetch_assoc($check))  //finds the rows that have our username
{
	  $usersilver = $row['silver'] ;
	
}

		
		$silver = $usersilver  - $cost;
		
            $query = "UPDATE silver SET  `silver`='".$silver."' WHERE `user_id`='".$userid."'"; 
            $result = mysql_query($query) or die('Query failed: ' . mysql_error()); 
			
			 mysql_query("INSERT INTO `player_collections` VALUES ('NULL', '{$userid}', '{$index}')") or die (mysql_error()); 
			
       // } 
	   }
	   }
?>