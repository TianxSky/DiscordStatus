<?php

if (isset($_GET['ip'])) {
    [$domainWithoutPort, $port] = explode(':', trim($_GET['ip'])) + [null, null];
    $ip = gethostbyname($domainWithoutPort);
    
    if ($ip !== $domainWithoutPort) {
        $steamConnectLink = "steam://connect/" . $ip . ":$port";
        header("Location: " . $steamConnectLink);
        exit;
    } else {
        header("Location: " . "steam://connect/" . $_GET['ip']);
        exit;
    }
}

?>
