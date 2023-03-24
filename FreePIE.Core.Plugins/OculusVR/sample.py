diagnostics.watch(oculusVR.isMounted)
diagnostics.watch(oculusVR.buttons)
diagnostics.watch(oculusVR.touches)

diagnostics.watch(oculusVR.headStatus)
diagnostics.watch(oculusVR.leftTouchStatus)
diagnostics.watch(oculusVR.rightTouchStatus)

diagnostics.watch(oculusVR.isHeadTracking)
diagnostics.watch(oculusVR.isLeftTouchTracking)
diagnostics.watch(oculusVR.isRightTouchTracking)

diagnostics.watch(oculusVR.headPose.yawDegrees)
diagnostics.watch(oculusVR.headPose.pitchDegrees)
diagnostics.watch(oculusVR.headPose.rollDegrees)

if oculusVR.isHeadTracking:
	diagnostics.watch(oculusVR.headPose.x)
	diagnostics.watch(oculusVR.headPose.y)
	diagnostics.watch(oculusVR.headPose.z)

diagnostics.watch(oculusVR.leftTouchPose.yawDegrees)
diagnostics.watch(oculusVR.leftTouchPose.pitchDegrees)
diagnostics.watch(oculusVR.leftTouchPose.rollDegrees)

if oculusVR.isLeftTouchTracking:
	diagnostics.watch(oculusVR.leftTouchPose.x)
	diagnostics.watch(oculusVR.leftTouchPose.y)
	diagnostics.watch(oculusVR.leftTouchPose.z)

diagnostics.watch(oculusVR.rightTouchPose.yawDegrees)
diagnostics.watch(oculusVR.rightTouchPose.pitchDegrees)
diagnostics.watch(oculusVR.rightTouchPose.rollDegrees)

if oculusVR.isRightTouchTracking:
	diagnostics.watch(oculusVR.rightTouchPose.x)
	diagnostics.watch(oculusVR.rightTouchPose.y)
	diagnostics.watch(oculusVR.rightTouchPose.z)