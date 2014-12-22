<?php
class Tag extends Illuminate\Database\Eloquent\Model {
    protected $fillable = ['name', 'short_name'];
    
    public function games() {
        return $this->belongsToMany('Game');
    }

    public function user() {
        return $this->belongsTo('User');
    }
}
