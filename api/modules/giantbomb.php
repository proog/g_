<?php

function gbBuildUrl($resource, array $queries) {
    $apiKey = Config::query()->firstOrFail()->giant_bomb_api_key;

    if(!$apiKey) {
        throw new Exception('No Giant Bomb API key specified. Please request an API key and update the giant_bomb_api_key attribute in the config table.');
    }

    $url = 'http://www.giantbomb.com/api/'.$resource.'/?format=json&api_key='.$apiKey;

    foreach($queries as $key => $value) {
        $url .= "&$key=".rawurlencode($value);
    }

    return $url;
}

function gbGetResults($url) {
    $context = stream_context_create([
        'http' => [
            'method' => 'GET',
            'header' => "User-Agent: permortensen.com g_ 0.1\r\n"
        ]
    ]);
    return json_decode(file_get_contents($url, null, $context))->results;
}

function gbGetGameById($id) {
    $url = gbBuildUrl('game/'.$id, [
        'field_list' => 'name,original_release_date,genres,platforms,image,developers,publishers'
    ]);
    $gb = gbGetResults($url);

    $year = null;
    if($gb->original_release_date) {
        $date = new DateTime($gb->original_release_date);
        $year = $date->format('Y');
    }

    $developer = count($gb->developers) > 0 ? $gb->developers[0]->name : null;
    $publisher = count($gb->publishers) > 0 ? $gb->publishers[0]->name : null;
    $imageUrl = $gb->image->small_url ?: null;

    $user = getLoggedInUserOrFail();

    // find common genres
    $genreIds = [];
    $availableGenres = $user->genres;
    foreach ($gb->genres as $gbGenre) {
        foreach ($availableGenres as $genre) {
            $lc1 = strtolower(str_replace(' ', '', $gbGenre->name));
            $lc2 = strtolower(str_replace(' ', '', $genre->name));
            $lc3 = strtolower(str_replace(' ', '', $genre->short_name));

            if(strstr($lc1, $lc2) !== false || strstr($lc1, $lc3) !== false || strstr($lc2, $lc1) !== false) {
                if(!in_array($genre->id, $genreIds)) {
                    $genreIds[] = $genre->id;
                }
            }
        }
    }

    // find common platforms
    $platformIds = [];
    $availablePlatforms = $user->platforms;
    foreach ($gb->platforms as $gbPlatform) {
        foreach ($availablePlatforms as $platform) {
            $lc1 = strtolower(str_replace(' ', '', $gbPlatform->name));
            $lc2 = strtolower(str_replace(' ', '', $platform->name));
            $lc3 = strtolower(str_replace(' ', '', $platform->short_name));

            if(strstr($lc1, $lc2) !== false || strstr($lc1, $lc3) !== false || strstr($lc2, $lc1) !== false) {
                if(!in_array($platform->id, $platformIds)) {
                    $platformIds[] = $platform->id;
                }
            }
        }
    }

    // if we found only one platform match, use that. Otherwise, use nothing,
    // because odds are it should only be added for one platform anyway
    if(count($platformIds) > 1) {
        $platformIds = [];
    }

    echo json_encode([
        'title' => $gb->name,
        'year' => is_numeric($year) ? (int)$year : null,
        'developer' => $developer,
        'publisher' => $publisher,
        'genre_ids' => $genreIds,
        'platform_ids' => $platformIds,
        'image_url' => $imageUrl
    ]);
}

function gbGetGamesByTitle($search) {
    $search = urldecode($search);
    $url = gbBuildUrl('games', [
        'filter' => 'name:'.$search,
        'field_list' => 'name,original_release_date,id',
        'limit' => 20
    ]);
    $results = gbGetResults($url);

    echo json_encode(array_map(function($result) {
        return [
            'id' => $result->id,
            'title' => $result->name
        ];
    }, $results));
}
