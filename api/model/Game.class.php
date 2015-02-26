<?php
class Game extends Illuminate\Database\Eloquent\Model {
    protected $fillable = ['title', 'developer', 'publisher', 'year', 'image', 'finished', 'comment', 'sort_as', 'playtime', 'rating', 'currently_playing', 'queue_position', 'hidden', 'wishlist_position'];
    protected $hidden = ['private_comment', 'genres', 'platforms', 'tags'];
    protected $appends = ['genre_ids', 'platform_ids', 'tag_ids'];
    
    const NOT_FINISHED = 0;
    const FINISHED = 1;
    const FINISHED_NA = 2;
    const SHELVED = 3;
    
    public function genres() {
        return $this->belongsToMany('Genre');
    }
    
    public function platforms() {
        return $this->belongsToMany('Platform');
    }
    
    public function tags() {
        return $this->belongsToMany('Tag');
    }
    
    public function user() {
        return $this->belongsTo('User');
    }

    public function getIdAttribute($value) {
        return (int) $value;
    }

    public function getYearAttribute($value) {
        return $value !== null ? (int) $value : null;
    }

    public function getFinishedAttribute($value) {
        return (int) $value;
    }

    public function getRatingAttribute($value) {
        return $value !== null ? (int) $value : null;
    }

    public function getCurrentlyPlayingAttribute($value) {
        return (boolean) $value;
    }

    public function getQueuePositionAttribute($value) {
        return $value !== null ? (int) $value : null;
    }

    public function getHiddenAttribute($value) {
        return (boolean) $value;
    }

    public function getWishlistPositionAttribute($value) {
        return $value !== null ? (int) $value : null;
    }

    public function getUserIdAttribute($value) {
        return (int) $value;
    }
    
    public function getGenreIdsAttribute() {
        $ids = [];
        foreach($this->genres as $genre)
            array_push($ids, (int)$genre->id);
        
        return $ids;
    }
    
    public function getPlatformIdsAttribute() {
        $ids = [];
        foreach($this->platforms as $platform)
            array_push($ids, (int)$platform->id);
        
        return $ids;
    }
    
    public function getTagIdsAttribute() {
        $ids = [];
        foreach($this->tags as $tag)
            array_push($ids, (int)$tag->id);
        
        return $ids;
    }
    
    public function addImage($image) {
        if(!is_uploaded_file($image['tmp_name']))
            return false;

        $split = explode('.', $image['name']);
        $filename = 'image.' . $split[count($split) - 1]; // image.ext
        $dir = '../public/images/'.$this->id;
        if(file_exists($dir))
            deleteDirectoryRecursively($dir);
        mkdir($dir);
        
        $fullpath = $dir.'/'.$filename;
        
        move_uploaded_file($image['tmp_name'], $fullpath);

        $this->image = 'images/'.$this->id.'/'.$filename;
        $this->save();
        
        return true;
    }
    
    public function deleteImage() {
        if(!$this->id)
            return;
        
        deleteDirectoryRecursively('../public/images/'.$this->id);
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
        if(!$this->title)
            throw new InvalidModelException('Invalid title');

        if(!$this->sort_as)
            throw new InvalidModelException('Invalid sorting title');

        if(!in_array($this->finished, [self::NOT_FINISHED, self::FINISHED, self::FINISHED_NA, self::SHELVED]))
            throw new InvalidModelException('Invalid finished state');
    }
}
