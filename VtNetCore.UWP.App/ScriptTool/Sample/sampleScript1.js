


function getSession(device)
{

}

/// Runs a command on a Cisco network device
/// @hostAddress the host address or IP address of the device
/// @startingMode the name of the mode to start in userExec, privExec and globalConfig
/// @commands a list of commands to run 
/// @return the output of each command executed
function executeCiscoCommands(device, startingMode, commands)
{
    var session = 
}


/// Maps the network starting at the given device
/// @hostAddress the IP address or hostname of the device to map
function mapNetworkFromDevice(hostAddress)
{
    // Attempt to find a record of the given device.
    var device = lookupDevice(hostAddress);

    if(device == null)
    {
        device = registerNewDevice(hostAddress);
    }
    else
    {
        // If the device has already been mapped or is currently being mapped,
        // don't try again.
        //
        // If the device failed to map, don't try again.
        if(device.isMapped || device.isMapping || device.mappingFailed)
            return;
    }

    device.isMapping = true;
    logger.DebugLine("Attemping to map device " + hostAddress);



    device.isMapped = true;
}
