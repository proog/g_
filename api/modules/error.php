<?php
function showNotFound() {
    throw new Exception('Route not found');
}

function showError(Exception $e) {
    $app = Slim\Slim::getInstance();
    
    if(is_a($e, 'Illuminate\Database\Eloquent\ModelNotFoundException'))
        $app->halt(404, json_encode(['message' => $e->getMessage()]));
    elseif(is_a($e, 'NotAuthenticatedException'))
        $app->halt(403, json_encode(['message' => $e->getMessage()]));
    elseif(is_a($e, 'InvalidModelException'))
        $app->halt(400, json_encode(['message' => $e->getMessage()]));
    
    $app->halt(500, json_encode(['message' => $e->getMessage()]));
}

class InvalidModelException extends Exception {
    public function __construct($message = 'Invalid model', $code = 0, $previous = null) {
        parent::__construct($message, $code, $previous);
    }
}