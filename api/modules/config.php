<?php

function getConfig() {
    $config = Config::with('defaultUser')->firstOrFail();
    echo $config->toJson();
}
