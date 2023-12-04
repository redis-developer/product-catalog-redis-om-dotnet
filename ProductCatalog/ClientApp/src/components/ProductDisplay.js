import React from 'react';
import Tooltip from '@mui/material/Tooltip';
import {Chip} from "@mui/material";

class ProductDisplay extends React.Component{
    constructor(props) {
        super(props);                
    }

    render(){
        return(
            <div className="card mb-2 box-shadow" style={{alignContent : 'center'}}>
                <img  src={this.props.product.imageUrl} alt={this.props.product.productDisplayName} style={{ height: '60%', width: '60%', alignSelf: 'center' }} />
                <div className="card-body">
                    <p className="card-text">{this.props.product.productDisplayName}</p>
                    <div style={{alignContent: "left"}}>
                        <b>View Similar:</b>
                    </div>                    
                    <div className="d-flex justify-content-between align-items-center">
                        <div className="btn-group">
                            <Tooltip title="Search for similar products by image." arrow>
                                <button type="button" className="btn btn-sm btn-outline-secondary" style={{fontSize: 12}} onClick={async ()=>this.props.queryByImage(this.props.product.imageUrl)}>By Image</button>
                            </Tooltip>
                            <Tooltip title="Search for similar products by text." arrow>
                                <button type="button" className="btn btn-sm btn-outline-secondary" style={{fontSize: 12}} onClick={async()=>this.props.queryByDescription(this.props.product.productDisplayName)}>By Text</button>
                            </Tooltip>
                        </div>
                        <div className="btn-group">
                            {this.props.product.score?(
                                <Tooltip title="Similarity Score" arrow>
                                    <Chip
                                        style={{margin: "auto", fontSize: 12}}
                                        label={this.props.product.score.toFixed(2)}
                                        color="primary"
                                    />
                                </Tooltip>
                            ):(<></>)}    
                        </div>
                    </div>    
                </div>                
            </div>
        )
    }    
}

export default ProductDisplay;