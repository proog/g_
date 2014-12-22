<?php

function getConfig() {
    $config = Config::with('defaultUser')->firstOrFail();
    echo $config->toJson(JSON_NUMERIC_CHECK);
}
