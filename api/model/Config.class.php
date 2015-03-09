<?php
class Config extends Illuminate\Database\Eloquent\Model {
    protected $table = 'config';
    protected $hidden = ['giant_bomb_api_key'];
    protected $appends = ['is_assisted_creation_enabled'];
    public $timestamps = false;

    public function defaultUser() {
        return $this->belongsTo('User', 'default_user', 'id');
    }

    public function getIdAttribute($value) {
        return (int) $value;
    }

    public function getIsAssistedCreationEnabledAttribute() {
        return (bool) $this->giant_bomb_api_key;
    }

    public function isValid()  {
        try {
            $this->validOrThrow();
        }
        catch(InvalidModelException $e) {
            return false;
        }

        return true;
    }

    public function validOrThrow() {
        if(!$this->defaultUser)
            throw new InvalidModelException('Invalid default user');
    }
}
